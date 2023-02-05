using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    public class CallTarget
    {
        public string target;
        public string methodName;
    }

    public class CallerData : ObjectData
    {
        private GUIStyle buttonStyle;

        public List<CallTarget> targets;
        [RenderData(2,"Targets")]
        public virtual void RenderTargets(int width)
        {
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button);
            }

            Color color = Color.black;
            string name = "null";
            foreach (CallTarget callTarget in targets)
            {
                if (string.IsNullOrEmpty(callTarget.target) )
                    color = Color.red;
                else
                    name = callTarget.methodName;

                if(string.IsNullOrEmpty(callTarget.methodName))
                    color = Color.red;
            }
            buttonStyle.normal.textColor = color;

            if (targets.Count > 1)
            {
                name += " + " + (targets.Count - 1) + " more";
            }

            if (!GUILayout.Button(name, buttonStyle)) return;

            GenericMenu menu = new GenericMenu();
            for (int index = 0; index < targets.Count; index++)
            {
                CallTarget target = targets[index];
                string txt = (index + 1).ToString() + " : ";

                txt += string.IsNullOrEmpty(target.target) ? "null" : target.methodName;
                menu.AddItem(new GUIContent(txt) , false,
                    () => { });
            }
            menu.ShowAsContext();
        }

    }
}