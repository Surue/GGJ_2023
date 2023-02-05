//Created by Antoine Pastor
//Old Skull Games
//31/08/2017

using OSG;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameObjectByPath))]
public class GameObjectByPathPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //object theObject;// = GetObject(property);
        //property.GetObjectValue(out theObject);
        //GameObjectByPath obj = fieldInfo.GetValue(theObject) as GameObjectByPath;
        
        GameObjectByPath obj;
        if (property.GetObjectValue(out obj) && obj.IsValid())
            return EditorGUIUtility.singleLineHeight;
        else 
            return EditorGUIUtility.singleLineHeight * 2;
    }

//    private static object GetObject(SerializedProperty property)
//    {
//        object theObject;
//        property.GetRealObjectFieldInfo(out theObject);
//        return theObject;
//    }
//
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
//        object theObject;
//        property.GetObjectValue(out theObject);
//        GameObjectByPath obj = fieldInfo.GetValue(theObject) as GameObjectByPath;
        GameObjectByPath obj;
        if(!property.GetObjectValue(out obj))
        {
            return;
        }
        
        if (!obj.IsValid())
            position.height *= 0.5f;
        
        obj.Set(EditorGUI.ObjectField(position, label.text, obj.Get(), typeof(GameObject), true) as GameObject);
        if (!obj.IsValid())
        {
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, "Cannot find object named "+obj.gameobjectName);   
        }
    }
}
