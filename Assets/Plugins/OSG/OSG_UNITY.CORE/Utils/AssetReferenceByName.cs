// Old Skull Games
// Bernard Barthelemy
// Wednesday, August 8, 2018

using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace OSG
{
    [Serializable]
    // allows to reference an asset by name
    public struct AssetReferenceByName
    {
        public string name;
        public string guid;
#if UNITY_EDITOR
        public UnityEngine.Object Asset
        {
            set
            {
                if (value)
                {
                    name = value.name;
                    string path = AssetDatabase.GetAssetPath(value);
                    guid = AssetDatabase.AssetPathToGUID(path);
                }
                else
                {
                    name = guid = null;
                }
            }
            get
            {
                if(string.IsNullOrEmpty(guid))
                {
                    name = null;
                    return null;
                }
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if(string.IsNullOrEmpty(path))
                {
                    guid = null;
                    name = null;
                    return null;
                }
                return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            }
        }
#endif
    }
}
