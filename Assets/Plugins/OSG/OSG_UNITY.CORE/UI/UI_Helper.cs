#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OSG
{
    public static class UI_Helper{
        
        public static void ApplyRectTransformOnGameObject(GameObject gameObj, RectTransform originalRectTransform){
            //Workaround for Unity bug losing anchors in nested canvases
            RectTransform newTransform = gameObj.GetComponent<RectTransform> ();
            newTransform.anchorMin = originalRectTransform.anchorMin;
            newTransform.anchorMax = originalRectTransform.anchorMax;
            newTransform.offsetMax = originalRectTransform.offsetMax;
            newTransform.offsetMin = originalRectTransform.offsetMin;
            newTransform.anchoredPosition = originalRectTransform.anchoredPosition;
        }

        public static T PopUIPrefabWithAnchors<T>(T prefab, Transform parent) where T : MonoBehaviour {
            RectTransform previousTransform = prefab.GetComponent<RectTransform> ();
            T newInstance = Object.Instantiate<T>(prefab, parent, false);

            ApplyRectTransformOnGameObject(newInstance.gameObject, previousTransform);
            return newInstance;
        }

        #if UNITY_EDITOR
        public static T PopUIPrefabWithAnchorsForEditor<T>(T prefab, Transform parent) where T : MonoBehaviour {
            RectTransform previousTransform = prefab.GetComponent<RectTransform> ();
            T newInstance = PrefabUtility.InstantiatePrefab(prefab) as T;
            newInstance.transform.SetParent (parent, false);

            ApplyRectTransformOnGameObject(newInstance.gameObject, previousTransform);
            return newInstance;
        }
        #if POPUP
        public static T InstantiateOnNewCanvas<T>(T prefab, float width) where T : MonoBehaviour{
            Canvas mainCanvas = PopupManager.FindMainCanvas ();
            GameObject subCanvasGO = new GameObject ();
            subCanvasGO.transform.SetParent (mainCanvas.gameObject.transform, false);
            subCanvasGO.name = "SubMenu parentCanvas";
            subCanvasGO.AddComponent<Canvas> ();
            RectTransform subCanvasRT = subCanvasGO.GetComponent<RectTransform> ();
            subCanvasRT.pivot = new Vector2 (0, 0.5f);
            subCanvasRT.anchorMin = new Vector2 (0, 0);
            subCanvasRT.anchorMax = new Vector2 (0, 1);
            subCanvasRT.offsetMin = new Vector2 (0, 0);
            subCanvasRT.anchoredPosition = new Vector2 (0, 0);
            subCanvasRT.sizeDelta = new Vector2 (width, 0);


            T theInstance = UI_Helper.PopUIPrefabWithAnchorsForEditor<T>(prefab, subCanvasGO.transform);

            return theInstance;
        }
        #endif
        #endif
    }
}
