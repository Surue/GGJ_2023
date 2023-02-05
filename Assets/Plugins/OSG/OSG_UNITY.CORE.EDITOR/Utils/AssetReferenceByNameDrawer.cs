// Old Skull Games
// Bernard Barthelemy
// Wednesday, August 8, 2018

using UnityEditor;
using UnityEngine;

namespace OSG
{
    [CustomPropertyDrawer(typeof(AssetReferenceByName))]
    public class AssetReferenceByNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty guid = property.FindPropertyRelative("guid");
            SerializedProperty name = property.FindPropertyRelative("name");
            Object asset = null;
            if(!string.IsNullOrEmpty(guid.stringValue))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid.stringValue);
                if(!string.IsNullOrEmpty(path))
                {
                    asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if(asset && asset.name != name.stringValue)
                    {
                        Debug.LogWarning("Outdated name, replacing " + name.stringValue + " with " + asset.name);
                        name.stringValue = asset.name;
                    }
                }
            }

            var newAsset = EditorGUI.ObjectField(position, 
                new GUIContent(property.displayName, name == null ? "NULL" : name.stringValue), 
                asset, typeof(Object), false);

            if (newAsset != asset)
            {
                if(newAsset)
                {
                    name.stringValue = newAsset.name;
                    string path = AssetDatabase.GetAssetPath(newAsset);
                    guid.stringValue = AssetDatabase.AssetPathToGUID(path);
                }
                else
                {
                    name.stringValue = null;
                    guid.stringValue = null;
                }
            }
        }

        /*
        [MenuItem("OSG/Check All Reference By Name")]
        public static void CheckAllAssetReferenceByName()
        {
            //wesh
        }
        */
    }
}
