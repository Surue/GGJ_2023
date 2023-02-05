using System.Collections;
using UnityEngine;

namespace OSG
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformScaler : MonoBehaviour
    {

        private RectTransform _rectTransform;
        private RectTransform rectTransform
        {
            get
            {
                if (!_rectTransform)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
        }

        [SerializeField] private RectTransform sourceRectTransform;
        [SerializeField] private Vector2 scaleFactor = Vector2.one;
        [SerializeField] private bool uniformScale = false;
        [SerializeField] private Fit fit = Fit.In;
        [SerializeField] private bool executeInUpdate = true;

#if UNITY_EDITOR
        [SerializeField] private bool editorPreview = true;
#endif

        private enum Fit
        {
            In,
            Out
        }

        void LateUpdate()
        {
            if (!sourceRectTransform)
                return;

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                if (!executeInUpdate)
                    return;
            }
            else if (!editorPreview)
                return;
#else
            if (executeInUpdate)
#endif
            CalculateScale();
        }

        public void CalculateScale()
        {
            float xScale = sourceRectTransform.rect.width / rectTransform.sizeDelta.x;
            float yScale = sourceRectTransform.rect.height / rectTransform.sizeDelta.y;

            if (uniformScale)
            {
                CompareScale(ref xScale, ref yScale);
                CompareScale(ref yScale, ref xScale);
            }

            rectTransform.localScale = new Vector3(xScale * scaleFactor.x, yScale * scaleFactor.y, 1);
        }

        public void CalculateScaleNextFrame()
        {
            if (!gameObject.activeSelf || !gameObject.activeInHierarchy)
                return;

            StartCoroutine(CalculateScaleNextFrameCoroutine());
        }

        protected IEnumerator CalculateScaleNextFrameCoroutine()
        {
            yield return new WaitForEndOfFrame();
            CalculateScale();
        }

        private void CompareScale(ref float a, ref float b)
        {
            a = (fit == Fit.In ? a < b : a > b) ? a : b;
        }
    }
}