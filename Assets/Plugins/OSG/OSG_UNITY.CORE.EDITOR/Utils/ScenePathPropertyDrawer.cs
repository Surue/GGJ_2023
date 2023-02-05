using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ScenePath))]
public class ScenePathPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SerializedProperty pathProperty = property.FindPropertyRelative(nameof(ScenePath.path));
        SerializedProperty guidProperty = property.FindPropertyRelative(nameof(ScenePath.guid));
        if (!string.IsNullOrEmpty(guidProperty.stringValue))
        {
            string path = AssetDatabase.GUIDToAssetPath(guidProperty.stringValue);
            if(pathProperty.stringValue != path)
                pathProperty.stringValue = path;
        }
        var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathProperty.stringValue);
        EditorGUI.BeginChangeCheck();
        var newScene = EditorGUI.ObjectField(position, label, 
                       oldScene, 
                       typeof(SceneAsset), false) as SceneAsset;
        if (EditorGUI.EndChangeCheck())
        {
            Object o = property.serializedObject.targetObject;
            Undo.RecordObject(o, "Change scenepath");
            string newPath = AssetDatabase.GetAssetPath(newScene);
            pathProperty.stringValue = newPath;
            string newGUID = AssetDatabase.AssetPathToGUID(newPath);
            guidProperty.stringValue = newGUID;
            EditorUtility.SetDirty(o);
        }
        EditorGUI.EndProperty();
    }
}
