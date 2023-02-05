// Old Skull Games
// Bernard Barthelemy
// Friday, May 19, 2017

using System;
using OSG.DebugTools;
using UnityEngine;

namespace OSG
{
    public static class RectTransformExtensions
    {
        public static Vector2 GetCenterFromScreenUIOverlayToScreenCoordinates(this RectTransform uiElement)
        {
            Rect result = uiElement.ConvertFromScreenUIOverlayToScreenCoordinates();
            return result.center;
        }
        
        public static Rect ConvertFromScreenUIOverlayToScreenCoordinates(this RectTransform uiElement)
        {
            var worldCorners = new Vector3[4];
            uiElement.GetWorldCorners(worldCorners);
            var result = new Rect(
                worldCorners[0].x,
                worldCorners[0].y,
                worldCorners[2].x - worldCorners[0].x,
                worldCorners[2].y - worldCorners[0].y);
            return result;
        } 

        public static void Draw(this Rect rect, Color color, Transform frame = null, float duration=0)
        {
            Vector3 topLeft, topRight, bottomLeft, bottomRight;

            topRight = topLeft = rect.min;
            topRight.x = rect.max.x;
            bottomLeft = bottomRight = rect.max;
            bottomLeft.x = rect.min.x;            

            if(frame)
            {
                topLeft = frame.TransformPoint(topLeft);
                topRight = frame.TransformPoint(topRight);
                bottomLeft = frame.TransformPoint(bottomLeft);
                bottomRight = frame.TransformPoint(bottomRight);
            }

            Debug.DrawLine(topLeft, topRight, color, duration);
            Debug.DrawLine(topRight, bottomRight, color, duration);
            Debug.DrawLine(bottomRight, bottomLeft, color, duration);
            Debug.DrawLine(bottomLeft, topLeft, color, duration);

            Debug.DrawLine(topLeft,bottomRight, color, duration);
            Debug.DrawLine(bottomLeft, topRight, color, duration);

        }

        public static Rect TransformRect(this RectTransform myTransform, Transform frame = null)
        {
            Rect rect = myTransform.rect;
            Vector3 min = myTransform.TransformPoint(rect.min);
            Vector3 max = myTransform.TransformPoint(rect.max);
            if(frame)
            {
                min = frame.InverseTransformPoint(min);
                max = frame.InverseTransformPoint(max);
            }

            float xMin = Mathf.Min(min.x, max.x);
            float yMin = Mathf.Min(min.y, max.y);
            float xMax = Mathf.Max(min.x, max.x);
            float yMax = Mathf.Max(min.y, max.y);

            min.x = xMin;
            min.y = yMin;
            min.z = 0;
            max.x = xMax;
            max.y = yMax;
            max.z = 0;
            return new Rect(min, max - min);
        }
    }
}