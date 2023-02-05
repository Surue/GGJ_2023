// Old Skull Games
// Bernard Barthelemy
// Friday, June 2, 2017

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using OSG.Core;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Serializes an object into a EditorPref's string
    /// </summary>
    [InitializeOnLoad]
    public static class EditorPrefsSerializer
    {
        private static readonly BinaryFormatter bf=new BinaryFormatter();
        static EditorPrefsSerializer()
        {
            // Don't use AssemblyScanner right away, this can
            // cause ugly side effects in loading resources...
            // don't ask...
            EditorListener.OnNextUpdateDo(Init);
        }

        private static void Init()
        {
            AssemblyScanner.Scan(LoadAll);
        }

        public static void LoadAll()
        {
            PrefContainer.LoadAll();
        }

        public static void SaveAll()
        {
            PrefContainer.SaveAll();
        }

        private static void SaveList(List<PrefToSave> list)
        {
            for (var index = 0; index < list.Count; index++)
            {
                list[index].Save();
            }
        }

        private static void LoadList(List<PrefToSave> list)
        {
            for (var index = 0; index < list.Count; index++)
            {
                list[index].Load();
            }
        }

        private class PrefToSave
        {
            public FieldInfo field;
            public EditorPrefsAttribute attribute;

            public MethodInfo onChangeInfo;

            public PrefToSave(FieldInfo f, EditorPrefsAttribute att)
            {
                field=f;
                attribute=att;
                if(string.IsNullOrEmpty(att.onChangeCallback))
                {
                    onChangeInfo=null;
                }
                else
                {
                    Type declaring = f.DeclaringType;
                    if(declaring!= null)
                    {
                        onChangeInfo = declaring.GetMethod(att.onChangeCallback, BindingFlags.Static | BindingFlags.NonPublic| BindingFlags.Public);
                    }
                }
            }

            private string GetKey()
            {
                if (field == null)
                {
                    return "";
                }

                return PlayerSettings.productName + "." + field.ReflectedType.Name + "." + field.Name;
            }

            public bool OnGUI()
            {
                if (!GenericInspector.OnGUI(field, attribute.label))
                    return false;
                Save();
                OnModificationCallBack();
                return true;
            }

            private void OnModificationCallBack()
            {
                if (onChangeInfo != null)
                {
                    onChangeInfo.Invoke(null, new object[] {field.Name});
                }
            }

            public void Save()
            {
                EditorPrefsSerializer.Save(GetKey(), field.GetValue(null));
            }

            public void Load()
            {
                string key = GetKey();
                if (EditorPrefs.HasKey(key))
                {
                    object value = field.GetValue(null);
                    EditorPrefsSerializer.Load(key, ref value);
                    try
                    {
                        field.SetValue(null, value);
                        OnModificationCallBack();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        EditorPrefs.DeleteKey(key);
                    }
                    
                }
            }
        }

        private class Container 
        {
            [EditorPrefs(showGUI = false)] private static Int32 currentTab;
            private static Vector2 scrollPos;

            private struct DataForType
            {
                public readonly List<PrefToSave> list;
                public readonly GUIContent content;
                public readonly bool hasGUI;

                public DataForType(Type type, List<PrefToSave> list)
                {
                    this.list = list;
                    content = new GUIContent(ObjectNames.NicifyVariableName(type.Name));
                    hasGUI = false;
                    for (var index = 0; index < list.Count && !hasGUI; index++)
                    {
                        hasGUI = list[index].attribute.showGUI;
                    }
                }
            }

            private List<DataForType> data;

            
            internal bool OnGUI()
            {
                if (data==null || data.Count <= 0)
                {
                    EditorGUILayout.HelpBox("No Editor Prefs defined in the project", MessageType.Error);
                    return false;
                }
                
                if ((uint) currentTab >= data.Count)
                {
                    currentTab = 0;
                }

                Rect r = EditorGUILayout.GetControlRect(false);

                if (EditorGUI.DropdownButton(r, data[currentTab].content, FocusType.Keyboard))
                {
                    GenericMenu menu = new GenericMenu();
                    for (var index = 0; index < data.Count; ++index)
                    {
                        if(!data[index].hasGUI) continue;
                        var index1 = index;
                        menu.AddItem(data[index].content, index == currentTab, () => currentTab = index1);
                    }
//                    menu.ShowAsContext();
                    menu.DropDown(r);
                }
                
                return OnGUI(data[currentTab].list);
            }

            bool OnGUI(List<PrefToSave> saves)
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                bool changed = false;
                for (var index= 0; index < saves.Count; index++)
                {
                    changed = saves[index].OnGUI() || changed;
                }
                GUILayout.EndScrollView();
                return changed;
            }

            public void Add(Type type, List<PrefToSave> prefsToSave)
            {
                data = data ?? new List<DataForType>(10);
                data.Add(new DataForType(type, prefsToSave));
            }

            internal void SaveAll()
            {
                if(data == null) return;
                try
                {
                    foreach (DataForType dataForType in data)
                    {
                        SaveList(dataForType.list);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }


            internal void LoadAll()
            {
                data.Sort((d1, d2) => string.CompareOrdinal(d1.content.text, d2.content.text));

                foreach (DataForType dataForType in data)
                {
                    LoadList(dataForType.list);
                }
            }
        }

        private static Container _prefsToSave;
        private static Container PrefContainer
        {
            get
            {
                return _prefsToSave ?? (_prefsToSave = new Container());
            }
        }

        [AssemblyScanner.ProcessTypeAttribute]
        public static void GetPrefsToSave(Type type)
        {
            List<PrefToSave> prefsToSave=null;
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fieldInfos)
            {
                if (Attribute.IsDefined(field, typeof(EditorPrefsAttribute)))
                {
                    Debug.LogError("Type " + type.Name.InBold() + " uses [EditorPrefs] on non static member " + field.Name.InBold());
                }
            }

            fieldInfos = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                var attributes = fieldInfo.GetCustomAttributes(false);
                foreach (var attribute in attributes)
                {
                    EditorPrefsAttribute att = attribute as EditorPrefsAttribute;
                    if (att != null)
                    {
                        prefsToSave = prefsToSave ?? new List<PrefToSave>();
                        prefsToSave.Add(new PrefToSave(fieldInfo, att));
                    }
                }
            }

            if (prefsToSave != null)
            {
                PrefContainer.Add(type, prefsToSave);
            }
        }
        public static void Save(string prefKey, object serializableObject)
        {
            if (serializableObject == null)
            {
                if (EditorPrefs.HasKey(prefKey))
                {
                    EditorPrefs.DeleteKey(prefKey);
                }
                return;
            }

            string val;

            try
            {
                MemoryStream memoryStream = new MemoryStream();
                bf.Serialize(memoryStream, serializableObject);
                val = Convert.ToBase64String(memoryStream.ToArray()) + "b";
            }
            catch
            {
                val = EditorJsonUtility.ToJson(serializableObject, false) + "j";
            }
            EditorPrefs.SetString(prefKey, val);
        }

        public static void Load(string prefKey, ref object overrideThis)
        {
            if (!EditorPrefs.HasKey(prefKey))
            {
                return;
            }
            string val = EditorPrefs.GetString(prefKey, string.Empty);
            if (string.IsNullOrEmpty(val))
                return;

            int index = val.Length - 1;
            char type = val[index];
            switch (type)
            {
                case 'b':
                    MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(val.Remove(index)));
                    overrideThis = bf.Deserialize(memoryStream);
                    break;
                case 'j':
                    EditorJsonUtility.FromJsonOverwrite(val.Remove(index), overrideThis);
                    break;
            }
        }

#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        static SettingsProvider PreferenceSettingsUI()
        {
            return new SettingsProvider("OSG/EditorPrefs", SettingsScope.User) {guiHandler = s => OnGUI()};
        }
#endif

        public static bool OnGUI()
        {
            return PrefContainer.OnGUI();
        }
    }

    class EditorPreferenceWindow : EditorWindow
    {
        [MenuItem("OSG/EditorPrefs")]
        public static void OpenPrefs()
        {
            EditorPrefsSerializer.LoadAll();
            GetWindow<EditorPreferenceWindow>(true, "Editor Prefs").Show();
        }

        void OnGUI()
        {
            if (EditorPrefsSerializer.OnGUI())
                Repaint();
        }

        void OnDisable()
        {
            EditorPrefsSerializer.SaveAll();
        }

    }


}