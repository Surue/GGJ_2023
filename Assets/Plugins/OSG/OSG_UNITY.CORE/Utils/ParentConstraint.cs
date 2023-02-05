using UnityEngine;
using OSG;

public class ParentConstraint : MonoBehaviour
{
    [SerializeField] private Transform anchor;
    [SerializeField] private Vector3 localPosition;

    [Space]
    [SerializeField] private bool useAnchorScale;
    [SerializeField] private Vector3 localScale = Vector3.one;

    void LateUpdate()
    {
        if (!anchor)
            return;

        transform.position = anchor.TransformPoint(localPosition);
        transform.rotation = anchor.rotation;

        if (useAnchorScale)
            transform.localScale = anchor.localScale.Multiply(localScale);
    }

    private bool CheckParent()
    {
        if (!anchor)
            return false;

        Transform[] children = anchor.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            // Transform is already a child of the anchor
            if (child == transform)
            {
                anchor = null;
                return false;
            }
        }

        return true;
    }

#if UNITY_EDITOR

    [HideInInspector] private Transform editorCacheAnchor;

    private void OnValidate()
    {
        if (editorCacheAnchor != anchor)
        {
            editorCacheAnchor = anchor;
            CheckParent();
        }
    }
#endif
}
