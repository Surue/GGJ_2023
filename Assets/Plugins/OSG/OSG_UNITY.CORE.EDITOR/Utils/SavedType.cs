using UnityEditor;

namespace OSG
{
    // TODO separate in Editor/ non editor classes
    public class EditorSavedBool : SavedType<bool>
    {
        public EditorSavedBool(string key, bool defaultValue=false) : base(key,defaultValue, EditorPrefs.SetBool, EditorPrefs.GetBool){}
    }

    public class EditorSavedInt : SavedType<int>
    {
        public EditorSavedInt(string key, int defaultValue = 0) : base(key, defaultValue, EditorPrefs.SetInt, EditorPrefs.GetInt){}
    }

    public class EditorSavedFloat : SavedType<float>
    {
        public EditorSavedFloat(string key, float defaultValue = 0) : base(key, defaultValue, EditorPrefs.SetFloat, EditorPrefs.GetFloat){}
    }

    public class EditorSavedString : SavedType<string>
    {
        public EditorSavedString(string key, string defaultValue ="") : base(key, defaultValue, EditorPrefs.SetString, EditorPrefs.GetString){}
    }

}