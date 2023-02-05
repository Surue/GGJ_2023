//Created by Antoine PASTOR
//OLD SKULL GAMES
//16/06/2017


using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace OSG
{
    /// <summary>
    /// Allows the user to lock the transform of a game object in the scene
    /// </summary>
    [InitializeOnLoad]
    public static class ObjectLocker
    {
#if UNITY_EDITOR

        [MenuItem("OSG/Lock/Lock transform")]
        public static void LockTransform()
        {
            LockSelected();
        }

        [MenuItem("OSG/Lock/Unlock transform")]
        public static void UnlockTransform()
        {
            UnlockSelected();
        }
        /// <summary>
        /// Locks the transform
        /// </summary>
        public static void LockSelected()
        {
            LockGameObjects(Selection.gameObjects);
        }

        /// <summary>
        /// use Selection.selectionChanged += OnSelectionChanged; to listen to selection changes and call this function to prevent
        /// the user from being able to move a locked object with handles
        /// </summary>
        static ObjectLocker()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        /// <summary>
        /// Locks the transform
        /// </summary>
        public static void LockGameObjects(GameObject[] objects)
        {
            for (var index = 0; index < objects.Length; index++)
            {
                GameObject anObject = objects[index];
                anObject.transform.hideFlags = HideFlags.NotEditable;
                EditorUtility.SetDirty(anObject);
            }
        }

        /// <summary>
        /// Unlocks the transform
        /// </summary>
        public static void UnlockSelected()
        {
            UnlockGameObjects(Selection.gameObjects);
        }

        /// <summary>
        /// Unlocks the transform
        /// </summary>
        public static void UnlockGameObjects(GameObject[] objects)
        {
            foreach (GameObject anObject in objects)
            {
                anObject.transform.hideFlags = HideFlags.None;
                EditorUtility.SetDirty(anObject);
            }
        }

        /// <summary>
        /// use Selection.selectionChanged += OnSelectionChanged; to listen to selection changes and call this function to prevent
        /// the user from being able to move a locked object with handles
        /// </summary>
        public static void OnSelectionChanged()
        {
            bool shouldLock = Selection.gameObjects.Any(anObject => anObject.transform.hideFlags == HideFlags.NotEditable);
            if (shouldLock)
            {
                Tools.hidden = true;
                Tools.current = Tool.None;
            }
            else
            {
                Tools.hidden = false;
                if (Tools.current == Tool.None)
                {
                    if (Selection.gameObjects.All(anObject => anObject.layer == LayerMask.NameToLayer("LD")))
                        Tools.current = Tool.Rect;
                    else
                        Tools.current = Tool.Move;
                }
            }
        }
#endif
    }

}

