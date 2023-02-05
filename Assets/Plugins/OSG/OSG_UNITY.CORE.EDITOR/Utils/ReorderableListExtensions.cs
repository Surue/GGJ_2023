using UnityEditorInternal;
using UnityEngine;

namespace OSG
{
    public static class ReorderableListExtensions
    {
        public static void DrawOnlyHeader(this ReorderableList list)
        {
            Rect headerRect = GUILayoutUtility.GetRect(0.0f, list.headerHeight, GUILayout.ExpandWidth(true));
            DrawOnlyHeader(list, headerRect);
        }

        public static void DrawOnlyHeader(this ReorderableList list, Rect headerRect)
        {
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                GUIStyle headerBackground = "RL Header";
                headerBackground.Draw(headerRect, false, false, false, false);
            }
            headerRect.xMin += 6f;
            headerRect.xMax -= 6f;
            headerRect.height -= 2f;
            ++headerRect.y;
            list.drawHeaderCallback(headerRect);
        }
    }
}