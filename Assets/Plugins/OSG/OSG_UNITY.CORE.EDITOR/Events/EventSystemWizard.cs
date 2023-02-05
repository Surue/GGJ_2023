// Old Skull Games
// Bernard Barthelemy
// Thursday, January 9, 2020


using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using OSG.Core;
using OSG.Core.EventSystem;
using OSG.Services;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace OSG.EventSystem.Editor
{
    [InitializeOnLoad]
    static class CheckForEventSystemToAdd
    {
        private static EditorSavedString baseName = new EditorSavedString("EventSystemBaseName");
        private static EditorSavedString path = new EditorSavedString("EventSystemPath");
        static CheckForEventSystemToAdd()
        {
            if (!string.IsNullOrEmpty(baseName))
            {
                EventSystemWizard.AddToDynamicServiceAsset(baseName, path);
                baseName.Value = "";
            }
        }

        public static void SetUp(string _baseName, string currentPath)
        {
            baseName.Value = _baseName;
            path.Value = currentPath;
        }
    }


    class EventSystemWizard : EditorWindow
    {
        private string eventSystemTemplate =
            "using OSG.Core.EventSystem;\n\npublic class %ES%: CoreEventSystem\n{\n    public class Ref : EventSystemRef<%EC%, %ES%>\n    {\n    }\n    public %ES%() : base(new %EC%())\n    {\n    }\n}\n";

        private string eventContainerTemplate =
            "using OSG.Core.EventSystem;\n\npublic class %EC% : CoreEventContainer\n{\n public readonly BoolEvent test = new BoolEvent();\n}";

        private string exampleTemplate =
            "public class %ES%Example\r\n{\r\n    %ES%.Ref eventSystem = new %ES%.Ref();\r\n\r\n    public void Register()\n    {\n        using (var events = eventSystem.RegisterContext(this))\n        {\n            events.test.AddListener(OnTest);\n        }\n    }\n\n    public void Emit()\n    {\r\n        eventSystem.Events.test.Invoke(true);\r\n    }\n\n    public void Unregister()\n    {\r\n        eventSystem.UnregisterFromAllEvents(this);\r\n    }\n\n    private void OnTest(bool value)\n    {\n        UnityEngine.Debug.Log($\"Received {value}\");\n    }\n}";

        [MenuItem("OSG/Event System Wizard")]
        private static void OpenWizard()
        {
            var wizard = EditorWindow.GetWindow<EventSystemWizard>();
            wizard.Show();
        }

        private string baseName;
        private string systemFile, systemName;
        private string containerFile, containerName;
        private bool addToDynamicServices = true;

        void OnGUI()
        {
            baseName = EditorGUILayout.TextField("Name", baseName);


            bool isNameValid = IsNameValid(baseName);

            GUI.color = GUI.enabled ? Color.white : Color.red;

            if (isNameValid)
            {
                Generate(baseName, out systemFile, out containerFile, out systemName, out containerName);
            }

            string path = GetCurrentPath();

            bool isPathValid = !string.IsNullOrEmpty(path);

            if (!isPathValid)
            {
                EditorGUILayout.HelpBox("Please select destination folder in Project", MessageType.Error);
            }

            if (!isNameValid)
            {
                EditorGUILayout.HelpBox("Please enter a valid identifier name", MessageType.Error);
            }

            if (isNameValid && isPathValid)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"{path}/{systemName}/{systemName}.cs : ");
                EditorGUILayout.TextArea(systemFile);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"{path}/{systemName}/{containerName}.cs : ");
                EditorGUILayout.TextArea(containerFile);
            }

            GUI.enabled = isNameValid && isPathValid;

            addToDynamicServices = EditorGUILayout.Toggle("Add to dynamic services", addToDynamicServices);

            if (GUILayout.Button("Generate"))
            {
                if (addToDynamicServices)
                {
                    //CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;
                    CheckForEventSystemToAdd.SetUp(baseName, GetCurrentPath());
                    
                }

                CreateFiles(path);
            }

            GUI.enabled = true;
        }

        public static void AddToDynamicServiceAsset(string baseName, string path)
        {
            string typeName = $"{baseName}EventSystem";
            var dsa = GetDynamicAsset(path);
            if (!dsa)
            {
                Debug.Log($"Could not get {DynamicServiceManager.AssetName}");
                return;
            }

            var found = false;

            StringBuilder s = new StringBuilder();
            AssemblyScanner.Register(type =>
            {
                if (!type.DerivesFrom(typeof(IEventSystem)))
                    return;

                s.AppendLine(type.FullName);
                if (type.Name == typeName)
                {
                    if (dsa.AddBinding(type, typeof(IEventSystem)))
                    {
                        EditorUtility.SetDirty(dsa);
                        found = true;
                    }
                }
            });
            AssemblyScanner.Scan(() =>
            {
                Debug.Log(s.ToString());
                if (found)
                {
                    Debug.Log($"{typeName} added to {dsa.name}");
                }
                else
                {
                    Debug.LogError($"Could not find type {typeName}");
                }
            });
        }

        private static DynamicServicesAsset GetDynamicAsset(string ifNotFoundCreateHere)
        {
            DynamicServicesAsset dsa = Resources.Load<DynamicServicesAsset>(DynamicServiceManager.AssetName);
            if (dsa)
            {
                return dsa;
            }

            string folder = DirectoryHelpers.Combine(ifNotFoundCreateHere, "Resources");
            if (!AssetDatabase.IsValidFolder(folder))
            {
                folder = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(ifNotFoundCreateHere, "Resources"));
            }
            dsa = ScriptableObjectUtility.CreateAssetAtPath<DynamicServicesAsset>(folder, DynamicServiceManager.AssetName);
            return dsa;
        }


        private void CreateFiles(string path)
        {
            string folder = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(path, systemName))
                .Substring("Assets/".Length);

            folder = DirectoryHelpers.Combine(Application.dataPath, folder);
            SaveFile(folder, $"{systemName}.cs", systemFile);
            SaveFile(folder, $"{containerName}.cs", containerFile);

            string exampleFile = exampleTemplate.Replace("%ES%", systemName);

            SaveFile(folder, $"{systemName}Example.cs", exampleFile);

            AssetDatabase.Refresh();
            Close();
        }

        private void SaveFile(string folder, string filename, string content)
        {
            string filePath = $"{folder}/{filename}";
            using (var file = File.CreateText(filePath))
            {
                file.Write(content);
            }
        }

        private static string GetCurrentPath()
        {
            Object[] selectedAsset = Selection.GetFiltered(typeof(DefaultAsset), SelectionMode.Assets);
            foreach (Object obj in selectedAsset)
            {
                return AssetDatabase.GetAssetPath(obj);
            }

            return "";
        }

        private void Generate(string baseName, out string systemFile, out string containerFile, out string systemName,
            out string containerName)
        {
            systemName = $"{baseName}EventSystem";
            containerName = $"{baseName}EventContainer";
            systemFile = eventSystemTemplate.Replace("%ES%", systemName).Replace("%EC%", containerName);
            containerFile = eventContainerTemplate.Replace("%ES%", systemName).Replace("%EC%", containerName);
        }

        Regex r = new Regex("[_A-Za-z][_A-Za-z0-9]*");

        private bool IsNameValid(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            Match match = r.Match(name);
            return match.Success && match.Value == name;
        }

    }
}