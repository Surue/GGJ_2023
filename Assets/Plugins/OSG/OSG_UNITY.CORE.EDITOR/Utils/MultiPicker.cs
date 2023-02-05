 using System;
 using UnityEngine;
 using UnityEditor;
 using System.Collections.Generic;
 using Object = UnityEngine.Object;

namespace OSG
{
    /// <summary>
    /// If the user presses the spacebar and left clics in the scene editor,
    /// a selection menu will appear if more than one object is under the cursor
    /// </summary>
    [InitializeOnLoad]
    public static class MultiPicker
    {
        static MultiPicker()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        enum SelectableType
        {
            UI,
            World,
            RayCast,
            NavMesh
        }

        struct Selectable
        {
            public GameObject gameObject;
            public SelectableType type;

            public void AddItem(int i, GenericMenu menu)
            {
                GameObject ob = gameObject;
                menu.AddItem(new GUIContent(i+" -"+ob.name + " (" + type + ")"), false, () => Selection.activeGameObject = ob);
            }
        }

        private static List<Selectable> selectables = new List<Selectable>();


        private static bool spacePressed = false;

        static void OnSceneGUI(SceneView sceneView)
        {
            var current = Event.current;
            if (current == null)
            {
                return;
            }

		
            if (current.type == EventType.KeyDown && KeyCode.Escape == current.keyCode)
            {
                spacePressed = false;
            }

            if (current.keyCode == KeyCode.Space)
            {
                if (current.type == EventType.KeyDown )
                {
                    spacePressed = true;
                    return;
                }
                if (current.type == EventType.KeyUp)
                {
                    spacePressed = false;
                    return;
                }
            }

            if (current.type == EventType.MouseDown && current.button == 0 && spacePressed)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                selectables.Clear();

                var rectTransforms = Object.FindObjectsOfType<RectTransform>();
                
                foreach (RectTransform transform in rectTransforms)
                {
                    if (transform.ContainsWorldPosition(ray.origin))
                    {
                        Add(transform.gameObject, SelectableType.UI);
                    }
                }

                var renderers = Object.FindObjectsOfType<Renderer>();
                foreach (Renderer rdr in renderers)
                {
                    if(rdr.transform is RectTransform)
                        continue;
                    Bounds bounds = rdr.bounds;
                    Vector3 worldPos = ray.origin;
                    worldPos.z = bounds.center.z;
                    if (bounds.Contains(worldPos))
                    {
                        Add(rdr.gameObject, SelectableType.World);
                    }
                }

                var hits = Physics.RaycastAll(ray, 10000);
                if (hits.Length > 1)
                {
                    Array.Sort(hits, (hit0, hit1) => (int)(1000.0f*(hit0.distance - hit1.distance)));
                    for (int i = 0; i < hits.Length; ++i)
                    {
                        Add(hits[i].collider.gameObject, SelectableType.RayCast);
                    }
                }

                MonoBehaviour[] pickables = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
                Vector2 mouseScreenPos = ray.origin;
                for (int i = 0; i < pickables.Length; ++i)
                {
                    MonoBehaviour mono = pickables[i];
                    if(!mono.gameObject.IsSelectableSceneObject()) continue;
                    IPickable pickable = mono as IPickable;
                    if(pickable==null) continue;
                    if (pickable.IsUnder(mouseScreenPos))
                    {
                        Add(mono.gameObject, SelectableType.NavMesh);
                    }
                }

                if(selectables.Count > 1)
                {
                    GenericMenu menu = new GenericMenu();
                    for (int i = 0; i < selectables.Count; ++i)
                    {
                        selectables[i].AddItem(i, menu);
                    }
                    menu.ShowAsContext();
                    current.Use();
                    spacePressed = false;
                }
            }
        }

        private static void Add(GameObject gameObject, SelectableType type)
        {
            

            foreach (Selectable selectable in selectables)
            {
                if (selectable.gameObject == gameObject)
                {
                    return;
                }
            }
            selectables.Add(new Selectable() {gameObject=gameObject, type = type});
        }
    }
}