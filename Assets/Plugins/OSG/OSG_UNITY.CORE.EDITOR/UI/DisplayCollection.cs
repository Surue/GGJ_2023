// // Old Skull Games
// // Bernard Barthelemy
// // 15:09

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    public class DisplayCollection<T>
    {
        public delegate bool FilterDelegate(string[] filters, T item);
        private readonly FilterDelegate filterDelegate;

        public delegate void DrawElementDelegate(T item);
        private readonly DrawElementDelegate drawElementDelegate;

        public delegate void SelectionDelegate(T item);

        private readonly float elementHeight;
        private readonly float scrollWidth;

        private readonly IEnumerable<T> allItems;
        private readonly List<T> showList;

        public DisplayCollection(IEnumerable<T> itemList, 
            DrawElementDelegate drawElement,
            FilterDelegate filterAction = null,
            float elementHeight = 16,
            float scrollWidth = 16
        )
        {
            allItems = itemList;
            filterDelegate = filterAction;
            this.elementHeight = elementHeight;
            this.scrollWidth = scrollWidth;
            drawElementDelegate = drawElement;
            showList = new List<T>(allItems.Count());
            CreateList();
        }

        private string _f="";
        private int firstDisplay;

        public string Filter
        {
            get { return _f; }
            set
            {
                if (value == null)
                    value = "";
                if (_f != value)
                {
                    _f = value;
                    CreateList();
                }
            }
        }

        private void CreateList()
        {
            showList.Clear();
            var split = Filter.ToUpper().Split(' ');
            foreach (var key in allItems)
            {
                if (string.IsNullOrEmpty(Filter) || filterDelegate==null || filterDelegate(split, key))
                {
                    showList.Add(key);
                }
            }
            firstDisplay = 0;
        }

        private Vector2 scrollPos;
        private int displayCount;
        public void OnGUI(Rect position)
        {
            try
            {
                Rect r = position;
                r.height = EditorGUIUtility.singleLineHeight;
                r.width -= scrollWidth;

                Rect searchRect = r;
                searchRect.y += 6;
                Filter = EditorGUIExtension.SearchField(searchRect, Filter);
                if(string.IsNullOrEmpty(Filter))
                {
                    Color c = GUI.color;
                    GUI.color = Color.gray;
                    searchRect.position += new Vector2(16,-2);
                    GUI.Label(searchRect,"Search");
                    GUI.color = c;
                }

                var current = Event.current;

                float space = 8;
                EditorGUILayout.GetControlRect(false, space);
                r.y += r.height + space;
                Rect scrollRect = position;
                scrollRect.yMin = r.yMin;
                scrollRect.xMin = r.xMax;

                displayCount = Mathf.FloorToInt(scrollRect.height / elementHeight) - 2;
                firstDisplay = Mathf.Clamp(firstDisplay, 0, showList.Count - 1);

                if (current != null && current.type == EventType.ScrollWheel)
                {
                    if(Math.Abs(current.delta.y) > 0.001f)
                        firstDisplay += (int) (current.delta.y * displayCount * 0.1f);
                }

                int lastDisplay = displayCount + firstDisplay;
                if (lastDisplay > showList.Count)
                    lastDisplay = showList.Count;

                position.xMax = scrollRect.xMin;
                position.yMin = scrollRect.yMin;
                position.height = scrollRect.height-8;

                DrawRect(position);

                //GUILayout.BeginArea(position);

                
                //scrollPos = GUILayout.BeginScrollView(scrollPos);
                GUI.color = Color.white;
                EditorGUILayout.BeginVertical();
                for (int i = firstDisplay; i < lastDisplay; ++i)
                {
                    drawElementDelegate(showList[i]);
                }

                EditorGUILayout.EndVertical();

                //GUILayout.EndScrollView();
                //GUILayout.EndArea();
                scrollRect.x += 5;
                firstDisplay = (int) GUI.VerticalScrollbar(scrollRect, firstDisplay, displayCount, 0, showList.Count);
            }catch{}
        }

        private static void DrawRect(Rect position)
        {
            return;
            GUI.color = Color.white;
            GUI.DrawTexture(position, Texture2D.whiteTexture);
            position.width -= 2;
            position.height -= 2;
            position.x += 1;
            position.y += 1;
            GUI.color = Color.blue;
            GUI.DrawTexture(position, Texture2D.whiteTexture);
        }
    }
}