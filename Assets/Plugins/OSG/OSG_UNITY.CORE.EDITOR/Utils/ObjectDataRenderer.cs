using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace OSG
{
    public class ObjectDataRenderer<TD> where TD : ObjectData
    {
        private List<ObjectData> dataList;
        private List<string> names;
        private List<MethodInfo> renderMethods;
        private Dictionary<MethodInfo, MethodInfo> sortDictionary;

        public ObjectDataRenderer(List<ObjectData> dataList)
        {
            sortedIndex = -1;
            this.dataList = dataList;
            renderMethods=new List<MethodInfo>();
            names = new List<string>();
            sortDictionary = new Dictionary<MethodInfo, MethodInfo>();

            List<MethodInfo> sortMethods = new List<MethodInfo>();

            foreach (MethodInfo method in typeof(TD).GetMethods())
            {
                object[] attributes = method.GetCustomAttributes(false);
                foreach (var customAttribute in attributes)
                {
                    RenderDataAttribute renderAttribute = customAttribute as RenderDataAttribute;
                    if (renderAttribute!= null)
                    {
                        renderMethods.Add(method);
                    }
                    else
                    {
                        SortDataAttribute sortAttribute = customAttribute as SortDataAttribute;
                        if (sortAttribute != null)
                        {
                            sortMethods.Add(method);
                        }
                    }
                }
            }
            renderMethods.Sort(MethodSort);

            foreach (MethodInfo renderMethod in renderMethods)
            {
                var renderAttributes = renderMethod.GetCustomAttributes(typeof(RenderDataAttribute), false);
                RenderDataAttribute renderAttribute = (RenderDataAttribute) renderAttributes[0];
                names.Add(renderAttribute.DisplayName);
                foreach (MethodInfo sortMethod in sortMethods)
                {
                    var sortAttribute = (SortDataAttribute)sortMethod.GetCustomAttributes(typeof(SortDataAttribute), false)[0];
                    if (sortAttribute.DisplayName == renderAttribute.DisplayName)
                    {
                        sortDictionary.Add(renderMethod, sortMethod);
                    }
                }
            }
        }

        private int MethodSort(MethodInfo x, MethodInfo y)
        {
            RenderDataAttribute atX = (RenderDataAttribute)x.GetCustomAttributes(typeof(RenderDataAttribute), false)[0];
            RenderDataAttribute atY = (RenderDataAttribute)y.GetCustomAttributes(typeof(RenderDataAttribute), false)[0];
            return atX.Priority - atY.Priority;
        }

        private Vector2 ScrollPos;

        private int sortedIndex;

        public void OnGUI()
        {
            if (dataList == null)
            {
                return;
            }

            if (!names.Any())
            {
                return;
            }

            GUILayout.Label("Found " + dataList.Count + " instances:");

            int width = Screen.width/names.Count;
            GUILayout.BeginHorizontal();
            for (int index = 0; index < names.Count; ++index)
            {
                MethodInfo sortMethod;
                if (sortDictionary.TryGetValue(renderMethods[index], out sortMethod))
                {
                    if (GUILayout.Button(names[index] + (sortedIndex==index ? " (*)" : ""), GUI.skin.label, GUILayout.Width(width)))
                    {
                        sortedIndex = index;
                        sortMethod.Invoke(dataList[0], new object[] {dataList});
                    }
                }
                else
                {
                    GUILayout.Label(names[index], GUILayout.Width(width));
                }
            }
            GUILayout.EndHorizontal();

            object[] ints = {width};

            ScrollPos = GUILayout.BeginScrollView(ScrollPos);

            foreach (TD d in dataList)
            {
                GUILayout.BeginHorizontal();
                for (int index = 0; index < renderMethods.Count; ++index)
                {
                    MethodInfo info = renderMethods[index];
                    info.Invoke(d, ints);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

    }
}
