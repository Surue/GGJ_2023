using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace OSG
{
    [CustomEditor(typeof(TextColors))]
    public class TextColorsEditor : Editor
    {
        private ReorderableList list;

        private void OnEnable()
        {
            list = new ReorderableList(serializedObject, serializedObject.FindProperty("colorNames"), true, true, true,
                true);
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, 80, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("Name"), GUIContent.none);
                EditorGUI.PropertyField(
                    new Rect(rect.x + 90, rect.y, rect.width - 80 - 30, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("Color"), GUIContent.none);
            };
            list.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Colors list");
            };
            list.onCanRemoveCallback = (ReorderableList l) => l.count > 1;
            list.onRemoveCallback = (ReorderableList l) =>
            {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the color?", "Yes", "No"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(l);
                }
            };
            list.onAddCallback = (ReorderableList l) =>
            {
                var index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;
                var element = l.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("Name").stringValue = "name";
                element.FindPropertyRelative("Color").colorValue = Color.white;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            //GUILayout.Label("instructions");
        }

    }
}