// Old Skull Games
// Bernard Barthelemy
// Tuesday, August 29, 2017

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OSG
{
    /// <summary>
    /// Helpers for game objects 
    /// </summary>
    public static class GameObjectExtensions
    {
        public static bool IsInLayerMask(this GameObject gameObject, LayerMask layerMask)
        {
            return 0 != (layerMask.value & (1 << gameObject.layer));
        }

        public static string HierarchyPath(this GameObject o)
        {
            return o.transform ? o.transform.HierarchyPath() : "";
        }

        public static string HierarchyPath(this Component b)
        {
            return b.transform.HierarchyPath();
        }

        public static void SetChildrenActive(this GameObject o, bool active)
        {
            o.transform.SetChildrenActive(active);
        }

        public static void DestroyAllChildren(this GameObject o)
        {
            o.transform.DestroyImmediateAllChildren();
        }
#if UNITY_EDITOR
        public static bool IsSelectableSceneObject(this Object o)
        {
            Component c = o as Component;
            // if we have a component and its root is persistent, 
            // it means we have a prefab, not in a scene
            if (c && EditorUtility.IsPersistent(c.transform.root)) return false;
            // non prefab components and other object types must have proper hideFlags
            return (o.hideFlags & (HideFlags.NotEditable | HideFlags.HideAndDontSave)) == 0;
        }
#endif

        public static T EnsureExists<T>(this GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            if (!c)
            {
                c = go.AddComponent<T>();
            }
            return c;
        }


        public static T[] FindObjectsOfTypeAll<T>()
        {
            List<T> results = new List<T>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                GameObject[] roots = SceneManager.GetSceneAt(i).GetRootGameObjects();
                for (int j = 0; j < roots.Length; j++)
                {
                    results.AddRange(roots[j].GetComponentsInChildren<T>());
                }
            }

            return results.ToArray();
        }

        public static Component EnsureExists(this GameObject go, Type componentType)
        {
            Component c = go.GetComponent(componentType);
            if (!c)
            {
                c = go.AddComponent(componentType);
            }

            return c;
        }


        /// <summary>
        /// Stops the given coroutine if it is running, then starts the routine and stores it in coroutine.
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="coroutine"></param>
        /// <param name="routine"></param>
        public static void StopAndStartCoroutine(this MonoBehaviour mono, ref Coroutine coroutine, IEnumerator routine)
        {
            if (coroutine != null)
            {
                mono.StopCoroutine(coroutine);
            }

            coroutine = mono.StartCoroutine(routine);
        }

        /// <summary>
        /// Stops the given coroutine if it is running, then sets it to null.
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="coroutine"></param>
        public static void DestroyCoroutine(this MonoBehaviour mono, ref Coroutine coroutine)
        {
            if (coroutine != null)
            {
                mono.StopCoroutine(coroutine);
            }

            coroutine = null;
        }
        /// <summary>
        /// GameObject.Find doesn't give a rat's ass to the current scene, so look after root manually
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject Find(this Scene scene, string name)
        {
            var roots = scene.GetRootGameObjects();

            foreach (GameObject gameObject in roots)
            {
                if (gameObject.name == name)
                {
                    return gameObject;
                }

                var tr = gameObject.transform.FindRecursive(name);
                if (tr)
                {
                    return tr.gameObject;
                }
            }
            return null;

        }

        public static Bounds GetMaxBounds(this GameObject g)
        {
            var b = new Bounds(g.transform.position, Vector3.zero);
            foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
            {
                b.Encapsulate(r.bounds);
            }

            return b;
        }


        public static void GetWorldCorners(this GameObject go, Vector3[] corners, Transform cameraTransform)
        {
            Bounds worldBounds = go.GetMaxBounds();
            Vector3 e = worldBounds.extents;
            Vector3[] points =
            {
                worldBounds.center + new Vector3(e.x, e.y, e.z),
                worldBounds.center + new Vector3(e.x, e.y, -e.z),
                worldBounds.center + new Vector3(e.x, -e.y, e.z),
                worldBounds.center + new Vector3(e.x, -e.y, -e.z),
                worldBounds.center + new Vector3(-e.x, e.y, e.z),
                worldBounds.center + new Vector3(-e.x, e.y, -e.z),
                worldBounds.center + new Vector3(-e.x, -e.y, e.z),
                worldBounds.center + new Vector3(-e.x, -e.y, -e.z),
            };

            var localBounds = new Bounds(cameraTransform.InverseTransformPoint(worldBounds.center), Vector3.zero);

            for (int i = 0; i < points.Length; ++i)
            {
                localBounds.Encapsulate(cameraTransform.InverseTransformPoint(points[i]));
            }

            // not tested: maybe b.min and b.max are ok, so use them
            // from unity doc : It starts bottom left and rotates to top left, then top right, and finally bottom right. x being left and y being bottom.
            Vector3 min = localBounds.min;
            Vector3 max = localBounds.max;
            corners[0] = new Vector3(min.x, min.y, min.z);
            corners[1] = new Vector3(min.x, max.y, min.z);
            corners[2] = new Vector3(max.x, max.y, min.z);
            corners[3] = new Vector3(max.x, min.y, min.z);
        }


        /// <summary>
        /// Return all gamobjects (with inactive) in a specified tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static GameObject[] FindObjectsWithTagAll(string tag)
        {
            List<GameObject> taggedObjects = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.isLoaded)
                {
                    GameObject[] rootGameobjects = s.GetRootGameObjects();

                    for (int j = 0; j < rootGameobjects.Length; j++)
                    {
                        taggedObjects.AddRange(FindObjectsWithTagAll(rootGameobjects[j], tag));
                    }
                }
            }

            return taggedObjects.ToArray();
        }

        public static GameObject[] FindObjectsWithTagAll(GameObject root, string tag)
        {
            List<GameObject> taggedObjects = new List<GameObject>();

            if (root.gameObject.tag == tag)
                taggedObjects.Add(root.gameObject);

            Transform transform = root.transform;

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform currentTransform = transform.GetChild(i);
                if (currentTransform.gameObject.tag == tag)
                    taggedObjects.Add(currentTransform.gameObject);

                taggedObjects.AddRange(FindObjectsWithTagAll(currentTransform.gameObject, tag));
            }

            return taggedObjects.ToArray();
        }


        /// <summary>
        /// Get all childs of a gameObject
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static GameObject[] GetChildren(GameObject root)
        {
            List<GameObject> childs = new List<GameObject>();

            Transform transform = root.transform;

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform currentTransform = transform.GetChild(i);
                childs.Add(currentTransform.gameObject);
                childs.AddRange(GetChildren(currentTransform.gameObject));
            }

            return childs.ToArray();
        }
    }
}
