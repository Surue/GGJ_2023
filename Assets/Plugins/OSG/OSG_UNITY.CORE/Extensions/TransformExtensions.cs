using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OSG
{
    public static class TransformExtensions
    {
        public static void PutAtMousePosition(this Transform t, Camera cam)
        {
            float zPos = t.position.z;
            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            t.position = new Vector3(r.origin.x, r.origin.y, zPos);
        }


        public static string HierarchyPath(this Transform transform, bool slashAtStart=true)
        {
            StringBuilder bb = new StringBuilder();
            while (transform)
            {
                bb.Insert(0, transform.name);
                transform = transform.parent;
                if (transform || slashAtStart)
                {
                    bb.Insert(0, '/');
                }
            }
            return bb.ToString();
        }

        public static void Reset(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        public static void ResetPositionAndRotation(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public static Transform FindRecursive(this Transform transform, string name)
        {
            if(transform.name == name)
                return transform;
            for(int i = transform.childCount; --i>=0;)
            {
                var c = transform.GetChild(i).FindRecursive(name);
                if(c)
                {
                    return c;
                }
            }
            return null;
        }

        public static void SetChildrenActive(this Transform transform, bool active)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(active);
            }
        }

        public static void DestroyImmediateAllChildren(this Transform transform)
        {
            int i = transform.childCount;
            while (--i >= 0)
            {
                GameObject o = transform.GetChild(i).gameObject;
                Object.DestroyImmediate(o);
            }
        }

        public static void DestroyAllChildren(this Transform transform)
        {
            int i = transform.childCount;
            while (--i >= 0)
            {
                GameObject o = transform.GetChild(i).gameObject;
                #if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    Object.DestroyImmediate(o);
                }
                else
                {
                    Object.Destroy(o);
                }
                #else
                Object.Destroy(o);
                #endif
                
                
            }
        }

        public static Canvas GetMainCanvas(this RectTransform transform)
        {
            RectTransform parent = transform.parent as RectTransform;
            if (parent)
            {
                Canvas parentCanvas = parent.GetMainCanvas();
                if (parentCanvas)
                {
                    return parentCanvas;
                }
            }

            Canvas canvas = transform.GetComponent<Canvas>();
            return canvas;
        }

        public static Vector3 LerpTowardsPosition(this Transform transform, Vector3 targetPosition, float lerp)
        {
            Vector3 positionLerp = Vector3.Lerp(transform.position, targetPosition, lerp);
            transform.position = positionLerp;
            return positionLerp;
        }

        public static Quaternion LerpTowardsRotation(this Transform transform, Quaternion targetRotation, float lerp)
        {
            return transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lerp);
        }

        public static Quaternion LerpTowardsRotation(this Transform transform, Vector3 targetRotation, float lerp)
        {
            return transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(targetRotation), lerp);
        }

        public static Quaternion LerpTowardsLocalRotation(this Transform transform, Quaternion targetRotation, float lerp)
        {
            return transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, lerp);
        }

        public static Quaternion LerpTowardsLocalRotation(this Transform transform, Vector3 targetRotation, float lerp)
        {
            return transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(targetRotation), lerp);
        }


        public static bool ContainsWorldPosition(this RectTransform rT, Vector3 worldPosition)
        {
            var checkPosition = rT.InverseTransformPoint(worldPosition);
            return rT.rect.Contains(checkPosition);
        }

        public static bool ContainsMouse(this RectTransform rT, Camera cam = null)
        {
            if(!cam)
                cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            return rT.ContainsWorldPosition(ray.origin);
        }

        public static bool ScreenPositionIsInsideRectTransform(this RectTransform rectTransform, Vector2 screenPosition)
        {
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;
            float scaleX = rectTransform.localScale.x;
            float scaleY = rectTransform.localScale.y;

            Vector2 anchoredPosition = rectTransform.anchoredPosition;
            Vector2 pivot = rectTransform.pivot;

            return new Rect(anchoredPosition.x - width * pivot.x * scaleX,
                            anchoredPosition.y - height * pivot.y * scaleY,
                            width * scaleX,
                            height * scaleY
                            ).Contains(screenPosition);
        }

        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }

        public static float GetWidth(this RectTransform rt)
        {
            return rt.rect.width;
        }

        public static float GetHeight(this RectTransform rt)
        {
            return rt.rect.height;
        }

        public static void SetHeight(this RectTransform rt, float height)
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        }

        public static void SetWidth(this RectTransform rt, float width)
        {
            rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
        }

        public static void AddTagRecursively(this Transform transform, string tag)
        {
            transform.gameObject.tag = tag;
            foreach (Transform t in transform)
            {
                AddTagRecursively(t, tag);
            }
        }
    }
}
