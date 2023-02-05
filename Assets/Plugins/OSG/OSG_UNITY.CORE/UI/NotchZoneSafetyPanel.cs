using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using OSG;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(RectTransform))]
public class NotchZoneSafetyPanel : MonoBehaviour
{
    [SerializeField, HideInInspector] private RectTransform rectTransform;

    [Tooltip("The RectTransform of the root canvas.")]
    [NotNull] [SerializeField] private RectTransform rootRectTransform;
    [Tooltip("The CanvasScaler of the root canvas.")]
    [NotNull] [SerializeField] private CanvasScaler rootCanvasScaler;

    [Space]
    [Tooltip("The default reference resolution that is applied to the root CanvasScaler in landscape mode. In portrait mode, the X and Y values are inverted.")]
    [SerializeField] private Vector2Int _landscapeResolution = new Vector2Int(1920, 1080);
    private Vector2Int landscapeResolution
    {
        get
        {
            return _landscapeResolution;
        }

        set
        {
            if (value.x < 0)
                value.x = 0;

            if (value.y < 0)
                value.y = 0;

            _landscapeResolution = value;
        }
    }
    [Tooltip("When ticked, safe area calculations are updated when the orientation of the device changes.")]
    [SerializeField] private bool dynamicOrientation;

    // Cache
    private Rect safeArea;

    private void Start()
    {
#if UNITY_EDITOR
        return;
#endif
        StartCoroutine(SetSafeArea());
    }

    private void Update()
    {
        if (!dynamicOrientation)
            return;

        if (safeArea != Screen.safeArea)
            StartCoroutine(SetSafeArea());
    }

    private IEnumerator SetSafeArea()
    {
        yield return null;

        safeArea = Screen.safeArea;

        SetCanvasScaler();
        SetRectTransform();
    }

    private void SetCanvasScaler()
    {
        rootCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        rootCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        rootCanvasScaler.referenceResolution = (safeArea.size.x > safeArea.size.y) ? // Landscape mode
            landscapeResolution : new Vector2(landscapeResolution.y, landscapeResolution.x);
    }

    private void SetRectTransform()
    {
        rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = Vector2.zero;
        rectTransform.anchoredPosition = safeArea.position / rootRectTransform.localScale;
        rectTransform.sizeDelta = safeArea.size / rootRectTransform.localScale;
    }

#if UNITY_EDITOR

    private NotchZoneSafetyPanel()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnSceneGUI(SceneView obj)
    {
        // Somehow, this object has been destroyed
        if (this == null)
        {
            SceneView.duringSceneGui -= OnSceneGUI;

            if (rectTransform)
                rectTransform.hideFlags = HideFlags.None;
            if (rootCanvasScaler)
                rootCanvasScaler.hideFlags = HideFlags.None;

            return;
        }

        if (!rectTransform)
            rectTransform = GetComponent<RectTransform>();

        rectTransform.hideFlags = HideFlags.NotEditable;
        if (rootCanvasScaler)
            rootCanvasScaler.hideFlags = HideFlags.NotEditable;

        if (EditorApplication.isPlaying)
            return;

        if (!rootRectTransform || !rootCanvasScaler)
            return;

        Rect safeArea = Screen.safeArea;

        if (this.safeArea != safeArea)
        {
            this.safeArea = safeArea;
            EditorSetSafeArea();
        }
    }

    private void OnValidate()
    {
        // Auto set
        if (!rectTransform)
            rectTransform = GetComponent<RectTransform>();

        // Hard set
        landscapeResolution = landscapeResolution;
    }


    private void EditorSetSafeArea()
    {
        SetCanvasScaler();
        EditorSetRectTransform();
    }

    private void EditorSetRectTransform()
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = Vector2.one * 0.5f;
        rectTransform.offsetMin = rectTransform.offsetMax = Vector2.zero;
    }

#endif
}
