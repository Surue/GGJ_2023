using System;
using System.Collections.Generic;
using System.Reflection;
using OSG.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OSG
{
    public class BaseObjectFinderWindow : EditorWindow
    {
        [MenuItem("OSG/Find/FinderWindow")]
        static void Init()
        {
            BaseObjectFinderWindow window = (BaseObjectFinderWindow) EditorWindow.GetWindow(typeof(BaseObjectFinderWindow), true,"Search");
            window.Show();
        }

        private string typeName = "";
        private static bool _typeSorted;
        private static int TypeComparer(Type x, Type y)
        {
            return string.CompareOrdinal(x.Name, y.Name);
        }


        public void OnGUI()
        {
            if (!_typeSorted)
            {
                AssemblyTypes.Sort(TypeComparer);
                _typeSorted = true;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Look for type:", GUILayout.Width(100));
        
            if(GUILayout.Button(">", GUILayout.Width(20)))
            {
                GenericMenu menu = new GenericMenu();
                foreach (Type type in AssemblyTypes)
                {
                    string entry = type.Name[0] + "/" + type.Name;
                    Type selectedType = type;
                    menu.AddItem(new GUIContent(entry),false,()=> typeName =  selectedType.Name);
                }
                menu.ShowAsContext();
            }

            typeName = EditorGUILayout.TextField(typeName);

            if (!string.IsNullOrEmpty(typeName) && GUILayout.Button("Search", GUILayout.Width(55)))
            {
                Search();
            }
            GUILayout.EndHorizontal();

            if (dataRenderer != null)
            {
                dataRenderer.OnGUI();
            }
        }

        private Type lookFor;

        private List<ObjectData> _objectDataList;
        private static ObjectDataRenderer<ObjectData> dataRenderer;

        private static List<Type> _assemblyTypes;

        private static List<Type> AssemblyTypes
        {
            get
            {
                if (_assemblyTypes != null) return _assemblyTypes;
                _assemblyTypes = new List<Type>();
                if (!EditorApplication.isPlaying)
                {
                    AssemblyScanner.Scan(null);
                }
                return _assemblyTypes;
            }
        }

        private static bool FilterAssemblyOut(Assembly assembly)
        {
            return !(assembly.FullName.StartsWith("Assembly-CSharp") || assembly.FullName.StartsWith("UnityEngine"));
        }

        [AssemblyScanner.ProcessType("FilterAssemblyOut")]
        static void ProcessType(Type type)
        {
            if (typeof(Object).IsAssignableFrom(type))
            {
                AssemblyTypes.Add(type);
            }
        }

        private void Search()
        {
            lookFor = AssemblyTypes.Find(type => type.Name == typeName);
            if (lookFor == null)
            {
                return;
            }

            _objectDataList = ObjectFinder<ObjectData, UnityEngine.Object>.FindAllObjectsInBuild(FindObjectOfType);
            dataRenderer = new ObjectDataRenderer<ObjectData>(_objectDataList);
        }

        private ObjectData FindObjectOfType(Object obj)
        {
            return obj.GetType() == lookFor || lookFor.IsInstanceOfType(obj) ? new ObjectData() : null;
        }
    }
}
