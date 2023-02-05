// Old Skull Games
// Pierre Planeau
// Tuesday, August 1, 2017

using System;
using System.Linq;
using UnityEngine;

namespace OSG
{
    public static class RectExtensions
    {
        public static Vector2 AnchorPos(this Rect rect, TextAnchor anchor, bool zeroOnBottom = false)
        {
            float left = rect.xMin;
            float center = rect.center.x;
            float right = rect.xMax;

            float upper = zeroOnBottom ? rect.yMin : rect.yMax;
            float middle = rect.center.y;
            float lower = zeroOnBottom ? rect.yMax : rect.yMin;
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return new Vector2(left, upper);
                case TextAnchor.UpperCenter: return new Vector2(center, upper);
                case TextAnchor.UpperRight: return new Vector2(right, upper);
                case TextAnchor.MiddleLeft: return new Vector2(left, middle);
                case TextAnchor.MiddleCenter: return rect.center;
                case TextAnchor.MiddleRight: return new Vector2(right, middle);
                case TextAnchor.LowerLeft: return new Vector2(left, lower);
                case TextAnchor.LowerCenter: return new Vector2(center, lower);
                case TextAnchor.LowerRight: return new Vector2(right, lower);
                default: throw new ArgumentOutOfRangeException();
            }
        }


        public static Rect FitTo(this Rect rect, Renderer renderer)
        {
            Bounds bounds = renderer.bounds;
            rect.center = bounds.min - renderer.transform.position;
            rect.size = bounds.size;

            return rect;
        }

        public static Rect[] HorizontalSplit(this Rect rect, params float[] ratios)
        {
            float totalRatio = ratios.Aggregate((total, next) => (total + next));
            Rect[] subRects = new Rect[ratios.Length];
            float x = rect.x;
            for (int i = 0; i < ratios.Length; i++)
            {
                Rect subRect = rect;
                subRect.x = x;
                subRect.width = rect.width * (ratios[i] / totalRatio);
                x += subRect.width;
                subRects[i] = subRect;
            }

            return subRects;
        }

        public static Rect AddMargin(this Rect rect, Vector2 margin)
        {
            Rect rectWithMargin = rect;
            
            rectWithMargin.width += margin.x;
            rectWithMargin.height += margin.y;
            rectWithMargin.x -= margin.x / 2;
            rectWithMargin.y -= margin.y / 2;

            return rectWithMargin;
        }
        
        public static Rect AddMargin(this Rect rect, float margin)
        {
            return rect.AddMargin(new Vector2(margin, margin));
        }


        /// <summary>
        /// Retrieves a rect with a specific X value.
        /// </summary>
        public static Rect SetX(this Rect rect, float x)
        {
            rect.x = x;
            return rect;
        }

        /// <summary>
        /// Offsets X value of Rect.
        /// </summary>
        public static Rect XOffset(this Rect rect, float offset)
        {
            rect.x += offset;
            return rect;
        }

        /// <summary>
        /// Retrieves a rect with a specific Y value.
        /// </summary>
        public static Rect SetY(this Rect rect, float y)
        {
            rect.y = y;
            return rect;
        }

        /// <summary>
        /// Offsets Y value of Rect.
        /// </summary>
        public static Rect YOffset(this Rect rect, float offset)
        {
            rect.y += offset;
            return rect;
        }

        /// <summary>
        /// Retrieves a rect with a specific width.
        /// </summary>
        public static Rect SetWidth(this Rect rect, float width)
        {
            rect.width = width;
            return rect;
        }

        /// <summary>
        /// Offsets Y value of Rect.
        /// </summary>
        public static Rect WidthOffset(this Rect rect, float offset)
        {
            rect.width += offset;
            return rect;
        }

        /// <summary>
        /// Retrieves a rect with a specific width.
        /// </summary>
        public static Rect SetHeight(this Rect rect, float height)
        {
            rect.height = height;
            return rect;
        }

        /// <summary>
        /// Offsets Y value of Rect.
        /// </summary>
        public static Rect HeightOffset(this Rect rect, float offset)
        {
            rect.height += offset;
            return rect;
        }


        /// <summary>
        /// Adds Rect values.
        /// </summary>
        public static Rect Add(this Rect rect, Rect otherRect)
        {
            rect.x += otherRect.x;
            rect.y += otherRect.y;
            rect.width += otherRect.width;
            rect.height += otherRect.height;
            return rect;
        }
    }
}