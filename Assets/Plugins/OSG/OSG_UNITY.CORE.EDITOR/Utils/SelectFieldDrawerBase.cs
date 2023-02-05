using OSG;
using UnityEngine;
using UnityEditor;


public abstract class SelectFieldDrawerBase<T> :  PropertyDrawer
{
    private DrawerFieldSelector<T> selector;
   
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        selector = new DrawerFieldSelector<T>(this, property);
        float propCount = 0;
        SelectedField.Enumerate(property, typeof(EditorShortcutKeys), s => ++propCount);
        return EditorGUIUtility.singleLineHeight * propCount;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if(selector != null)
        {
            selector.OnGUI(position);
        }
        property.serializedObject.ApplyModifiedProperties();
    }
}