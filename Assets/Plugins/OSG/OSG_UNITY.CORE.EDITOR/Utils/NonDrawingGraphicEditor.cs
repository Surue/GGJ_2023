// Old Skull Games
// Bernard Barthelemy from https://answers.unity.com/users/153967/slippdouglas.html
// Wednesday, January 31, 2018

using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace OSG
{
    [CanEditMultipleObjects, CustomEditor(typeof(NonDrawingGraphic), false)]
    public class NonDrawingGraphicEditor : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            base.serializedObject.Update();
            EditorGUILayout.PropertyField(base.m_Script, new GUILayoutOption[0]);
            // skipping AppearanceControlsGUI
            base.RaycastControlsGUI();
            base.serializedObject.ApplyModifiedProperties();
        }
    }
}