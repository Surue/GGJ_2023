using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace OSG
{
    public class ButtonsChecker : EditorWindow
    {
        [MenuItem("OSG/Check/Buttons")]
        static void Init()
        {
            Check();
            ButtonsChecker window = (ButtonsChecker) EditorWindow.GetWindow(typeof(ButtonsChecker));
            window.Show();
        }


        //[PostProcessScene]
        //public static void PostProcessScene()
        //{
        //    Check();
        //}


        private static List<ObjectData> _objectDatasArray;

        private static void Check()
        {
            _objectDatasArray = ObjectFinder<CallerData, Button>.FindAllObjectsInBuild(ButtonWithOnClickSelector);
            dataRenderer = new ObjectDataRenderer<CallerData>(_objectDatasArray);
        }

        private static ObjectDataRenderer<CallerData> dataRenderer;

        private static CallerData ButtonWithOnClickSelector(Button button)
        {
            var buttonClickedEvent = button.onClick;
            int listenerCount = buttonClickedEvent.GetPersistentEventCount();
            if (listenerCount <= 0)
                return null;

            List<CallTarget> targets = new List<CallTarget>(listenerCount);
            for (int i = 0; i < listenerCount; ++i)
            {
                Object listener = buttonClickedEvent.GetPersistentTarget(i);
                string methodName = buttonClickedEvent.GetPersistentMethodName(i);
                targets.Add(new CallTarget() {methodName = methodName, target = listener ? listener.name : null});
            }
            return new CallerData()
            {
                targets = targets
            };
        }

        private Vector2 scrollPos;
        GUIStyle labelStyle;

        void OnGUI()
        {
            if (_objectDatasArray == null)
            {
                return;
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Get called methods list"))
            {
                HashSet<string> calledMethodNames = new HashSet<string>();
                foreach (ObjectData data in _objectDatasArray)
                {
                    CallerData callerData = data as CallerData;
                    if(callerData==null)
                        continue;
                    foreach (CallTarget callTarget in callerData.targets)
                    {
                        if(string.IsNullOrEmpty(callTarget.target))
                            continue;
                        if(string.IsNullOrEmpty(callTarget.methodName))
                            continue;
                        calledMethodNames.Add(callTarget.methodName);
                    }
                }
                List<string> list = calledMethodNames.ToList();
                list.Sort();
                string allCalls="";
                foreach (string s in list)
                {
                    allCalls += "  - " + s + "\n";
                }

                GUIUtility.systemCopyBuffer = allCalls;
            }
            GUILayout.EndHorizontal();        
            dataRenderer.OnGUI();
        }

        private static string GetName(string location, Object obj)
        {
            string name = location;
            Component c = obj as Component;
            name += c ? ' ' + c.gameObject.HierarchyPath() : obj.name;
            return name;
        }
    }
}
