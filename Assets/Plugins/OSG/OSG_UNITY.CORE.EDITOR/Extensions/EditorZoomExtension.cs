// Old Skull Games
// Benoit Constantin
// Monday, February 25, 2019

using UnityEngine;
using System;
using UnityEditor;

namespace OSG
{
    public static partial class EditorGUIExtension
    {
        /// <summary>
        /// Make a zoomField for Handle / gui, you can scrollWheel to update the zoom value
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="drawCallback"></param>
        /// <param name="zoom"></param>
        /// <param name="handle"></param>
        /// <param name="gui"></param>
        /// <param name="sensitivity"></param>
        /// <returns></returns>
        public static Vector2 ZoomField(Rect rect, Action drawCallback, Vector2 zoom, bool handle = true, bool gui = true, float sensitivity = 0.5f)
        {
            Matrix4x4 baseHandleMatrix = Handles.matrix;
            Matrix4x4 baseGUIMatrix = GUI.matrix;

            if (handle)
                Handles.matrix *= Matrix4x4.Scale(new Vector3(zoom.x, zoom.y, 1));
            if (gui)
                GUI.matrix *= Matrix4x4.Scale(new Vector3(zoom.x, zoom.y, 1));


            drawCallback();

            Handles.matrix = baseHandleMatrix;
            GUI.matrix = baseGUIMatrix;

            if (Event.current.type == EventType.ScrollWheel && rect.Contains(Event.current.mousePosition))
            {
                zoom.x += Event.current.delta.y * sensitivity;
                zoom.y += Event.current.delta.y * sensitivity;

                zoom.x = Mathf.Max(zoom.x, 0.000001f);
                zoom.y = Mathf.Max(zoom.x, 0.000001f);

                GUI.changed = true;
            }

            return zoom;
        }


        /// <summary>
        ///  Make a offsetField for Handle/gui where you can drag element
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="drawCallback"></param>
        /// <param name="offset"></param>
        /// <param name="handle"></param>
        /// <param name="gui"></param>
        /// <param name="sensitivity"></param>
        /// <returns></returns>
        public static Vector2 OffsetField(Rect rect, Action drawCallback, Vector2 offset, bool handle = true, bool gui = true, float sensitivity = 0.5f)
        {
            Matrix4x4 baseHandleMatrix = Handles.matrix;
            Matrix4x4 baseGUIMatrix = GUI.matrix;

            if (handle)
                Handles.matrix *= Matrix4x4.Translate(offset);
            if (gui)
                GUI.matrix *= Matrix4x4.Translate(offset);

            drawCallback();

            Handles.matrix = baseHandleMatrix;
            GUI.matrix = baseGUIMatrix;

            if (Event.current.type == EventType.MouseDrag && rect.Contains(Event.current.mousePosition))
            {
                offset += Event.current.delta * sensitivity;
                GUI.changed = true;
            }

            return offset;
        }
    }
}
