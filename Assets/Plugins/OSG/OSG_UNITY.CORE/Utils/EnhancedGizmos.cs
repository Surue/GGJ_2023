
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using System;

namespace OSG
{
    using static EnhancedMath;

    public class EnhancedGizmos
    {
        public static Camera camera
        {
            get
            {
                #if UNITY_EDITOR
                return SceneView.lastActiveSceneView.camera;
                #else
                return null;
                #endif
            }
        }
        public static Transform view
        {
            get
            {
                return camera.transform;
            }
        }

        public const float defaultDottedScreenSpaceSize = 2;
        public const float thinDottedScreenSpaceSize = 1.5f;

        public const float defaultLineWidth = 1.5f;

        public enum LabelPosition
        {
            Start,
            Center,
            End
        }

        public class CustomLabel
        {
            public string text;
            public LabelPosition labelPosition;

            public CustomLabel (string text, LabelPosition labelPosition)
            {
                this.text = text;
                this.labelPosition = labelPosition;
            }
        }


        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Sets the same color for Gizmos and Handles.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void SetGizmosAndHandlesColor(Color color)
        {
#if UNITY_EDITOR
            Gizmos.color = Handles.color = color;
#endif
        }

        /// <summary>
        /// Returns Gizmos color with specified alpha.
        /// </summary>
        public static Color GizmosAlphaColor(float alpha)
        {
            Color color = Gizmos.color;
            color.a = alpha;

            return color;
        }

        /// <summary>
        /// Returns Handles color with specified alpha.
        /// </summary>
        public static Color HandlesAlphaColor(float alpha)
        {
            #if UNITY_EDITOR
            Color color = Handles.color;
            #else
            Color color = Color.white;
            #endif
            color.a = alpha;

            return color;
        }


        /// <summary>
        /// Draw an outlined circle.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawOutlinedCircle(Vector3 position, Vector3 normal, float radius, bool fillCircle = true, bool innerDot = false)
        {
#if UNITY_EDITOR
            // Cache
            Color startColor = Handles.color;
            float reducedAlpha = 0.5f;
            float innerDotRadius = 0.125f;

            // Wire disc(s)
            Handles.DrawWireDisc(position, normal, radius);
            if (innerDot)
            {
                SetGizmosAndHandlesColor(HandlesAlphaColor(startColor.a * reducedAlpha));
                Handles.DrawWireDisc(position, normal, radius * innerDotRadius);
            }

            // Solid disc(s)
            SetGizmosAndHandlesColor(startColor * reducedAlpha);

            if (fillCircle)
                Handles.DrawSolidDisc(position, normal, radius);
            if (innerDot)
            {
                SetGizmosAndHandlesColor(HandlesAlphaColor(startColor.a * reducedAlpha * reducedAlpha));
                Handles.DrawSolidDisc(position, normal, radius * innerDotRadius);
            }

            SetGizmosAndHandlesColor(startColor);
#endif
        }

        /// <summary>
        /// Draw an outlined circle with a specified color.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawOutlinedCircle(Vector3 position, Vector3 normal, float radius, Color color, bool fillCircle = true, bool innerDot = false)
        {
#if UNITY_EDITOR
            Color startColor = Handles.color;
            SetGizmosAndHandlesColor(color);

            DrawOutlinedCircle(position, normal, radius, fillCircle, innerDot);

            SetGizmosAndHandlesColor(startColor);
#endif
        }

        /// <summary>
        /// Draw a view-facing outlined circle.
        /// </summary>
        public static void DrawFacingOutlinedCircle(Vector3 position, float radius, bool fillCircle = true, bool innerDot = false)
        {
            DrawOutlinedCircle(position, ViewForward(position), radius, fillCircle, innerDot);
        }

        /// <summary>
        /// Draw a view-facing outlined circle with a specified color.
        /// </summary>
        public static void DrawFacingOutlinedCircle(Vector3 position, float radius, Color color, bool fillCircle = true, bool innerDot = false)
        {
            DrawOutlinedCircle(position, ViewForward(position), radius, color, fillCircle, innerDot);
        }


        /// <summary>
        /// Draws an outlined capsule-shaped polygon.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawCapsuleWithOutline(Vector3 position1, float radius1, Vector3 position2, float radius2, int subdivisions, bool drawDirectionArrow = false, CustomLabel label = null, float[] normalizedSteps = default)
        {
#if UNITY_EDITOR
            // Cache
            Color startColor = Handles.color;
            Vector3[] verticesArray = GetViewCapsuleVertices(position1, radius1, position2, radius2, subdivisions).ToArray();
            bool hasSteps = normalizedSteps != default && normalizedSteps.Length > 0;
            if (hasSteps)
                Array.Sort(normalizedSteps);

            // Label
            if (label != null)
            {
                Vector3 labelPosition = position1 + (position2 - position1) * ((float)label.labelPosition * 0.5f);
                Handles.Label(labelPosition, label.text);
            }

            // Outline
            Handles.DrawAAPolyLine(verticesArray);

            // Inner circles
            SetGizmosAndHandlesColor(HandlesAlphaColor(startColor.a * 0.5f));

            bool drawInnerDots = true;

            DrawFacingOutlinedCircle(position1, radius1, false, drawInnerDots);
            DrawFacingOutlinedCircle(position2, radius2, false, drawInnerDots);

            if (hasSteps)
            {
                for (int step = 0; step < normalizedSteps.Length; step++)
                {
                    float lerp = normalizedSteps[step];

                    if (lerp >= 1f)
                        continue;

                    DrawFacingOutlinedCircle(Vector3.Lerp(position1, position2, lerp), Mathf.Lerp(radius1, radius2, lerp), false, drawInnerDots);
                }
            }

            // Direction arrow
            if (drawDirectionArrow && (position2 - position1).magnitude > 0f)
            {

                float minRadius = radius1 < radius2 ? radius1 : radius2;

                Color arrowColor = HandlesAlphaColor(startColor.a * 0.5f);

                if (hasSteps)
                {
                    for (int step = 0; step < normalizedSteps.Length; step++)
                    {
                        Vector3 previousPosition = step == 0 ? position1 : Vector3.Lerp(position1, position2, normalizedSteps[step - 1]);
                        Vector3 lerpPosition = Vector3.Lerp(position1, position2, normalizedSteps[step]);

                        if ((lerpPosition - previousPosition).magnitude > 0f)
                            DrawFacingArrow(previousPosition, lerpPosition, minRadius, arrowColor);

                        // Last step
                        if (step == normalizedSteps.Length - 1 && normalizedSteps[step] < 1f)
                        {
                            DrawFacingArrow(lerpPosition, position2, minRadius, arrowColor);
                        }

                    }
                }
                else
                {
                    DrawFacingArrow(position1, position2, minRadius, arrowColor);
                }
            }

            // Fill polygon
            Handles.DrawAAConvexPolygon(verticesArray);

            SetGizmosAndHandlesColor(startColor);
#endif
        }

        /// <summary>
        /// Draws an outlined capsule-shaped polygon with a specific color.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawCapsuleWithOutline(Vector3 position1, float radius1, Vector3 position2, float radius2, int subdivisions, Color color, bool drawDirectionArrow = false, CustomLabel label = null, float[] normalizedSteps = default)
        {
#if UNITY_EDITOR
            Color startColor = Handles.color;
            SetGizmosAndHandlesColor(color);

            DrawCapsuleWithOutline(position1, radius1, position2, radius2, subdivisions, drawDirectionArrow, label, normalizedSteps);

            SetGizmosAndHandlesColor(startColor);
#endif
        }


        /// <summary>
        ///  Draws an outlined arrow between two points.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawArrow(Vector3 startPosition, Vector3 endPosition, Vector3 normal, float arrowSize, CustomLabel label = null)
        {
#if UNITY_EDITOR
            // Cache
            Color startColor = Handles.color;
            Vector3[] verticesArray = GetArrowVertices(startPosition, endPosition, normal, arrowSize).ToArray();

            // Label
            if (label != null)
            {
                Vector3 labelPosition = startPosition + (endPosition - startPosition) * ((float)label.labelPosition * 0.5f);
                Handles.Label(labelPosition, label.text);
            }

            // Gizmos
            Handles.DrawAAPolyLine(verticesArray);

            SetGizmosAndHandlesColor(HandlesAlphaColor(startColor.a * 0.5f));

            Handles.DrawAAConvexPolygon(verticesArray);

            SetGizmosAndHandlesColor(startColor);
#endif
        }

        /// <summary>
        ///  Draws an outlined arrow between two points with a specific color.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawArrow(Vector3 startPosition, Vector3 endPosition, Vector3 normal, float arrowSize, Color color, CustomLabel label = null)
        {
#if UNITY_EDITOR
            Color startColor = Handles.color;
            SetGizmosAndHandlesColor(color);

            DrawArrow(startPosition, endPosition, normal, arrowSize, label);

            SetGizmosAndHandlesColor(startColor);
#endif
        }

        /// <summary>
        ///  Draws a view-facing outlined arrow between two points.
        /// </summary>
        public static void DrawFacingArrow(Vector3 startPosition, Vector3 endPosition, float arrowSize, CustomLabel label = null)
        {
            DrawArrow(startPosition, endPosition, default, arrowSize, label);
        }

        /// <summary>
        ///  Draws a view-facing outlined arrow between two points with a specific color.
        /// </summary>
        public static void DrawFacingArrow(Vector3 startPosition, Vector3 endPosition, float arrowSize, Color color, CustomLabel label = null)
        {
            DrawArrow(startPosition, endPosition, default, arrowSize, color, label);
        }


        /// <summary>
        /// Draws a dotted line with the specified screen space size and speed.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawAnimatedDottedLine(Vector3 p1, Vector3 p2, float screenSpaceSize, float speed = 1f)
        {
#if UNITY_EDITOR
            if (screenSpaceSize < 0f)
                screenSpaceSize = 0f;

            float handleSize = HandleUtility.GetHandleSize(p1);
            float spacing = Mathf.Clamp(handleSize * screenSpaceSize / 20f, 0f, (p2 - p1).magnitude);
            float timeLerp = (float)EditorApplication.timeSinceStartup * handleSize * speed  * 0.2f % spacing / spacing;
            Vector3 offset = (p2 - p1).normalized * spacing;

            Handles.DrawDottedLine(p1, p1 + offset * Mathf.Clamp(timeLerp - 0.5f, 0f, 1f), screenSpaceSize);
            Handles.DrawDottedLine(p1 + offset * timeLerp, p2, screenSpaceSize);
#endif
        }


        /// <summary>
        /// Draws a dotted rect.
        /// </summary>
        public static void DrawDottedRect(Vector3 center, Quaternion rotation, float cameraDistance, float FOV, bool fill)
        {
            DrawDottedRect(center, rotation, cameraDistance, FOV, defaultDottedScreenSpaceSize, fill);
        }

        /// <summary>
        /// Draws a dotted rect.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawDottedRect(Vector3 center, Quaternion rotation, float cameraDistance, float FOV, float screenSpaceSize = defaultDottedScreenSpaceSize, bool fill = false)
        {
#if UNITY_EDITOR
            DrawDottedRect(Handles.color, center, rotation, cameraDistance, FOV, screenSpaceSize, fill);
#endif
        }

        /// <summary>
        /// Draws a dotted rect with the specified color.
        /// </summary>
        public static void DrawDottedRect(Color color, Vector3 center, Quaternion rotation, float cameraDistance, float FOV, bool fill)
        {
            DrawDottedRect(color, center, rotation, cameraDistance, FOV, defaultDottedScreenSpaceSize, fill);
        }

        /// <summary>
        /// Draws a dotted rect with the specified color.
        /// </summary>
        public static void DrawDottedRect(Color color, Vector3 center, Quaternion rotation, float cameraDistance, float FOV, float screenSpaceSize = defaultDottedScreenSpaceSize, bool fill = false)
        {
            DrawDottedRect(color, center, rotation, GetScreenFrustumAtDistance(FOV, cameraDistance), screenSpaceSize, fill);
        }

        /// <summary>
        /// Draws a dotted rect.
        /// </summary>
        public static void DrawDottedRect(Vector3 center, Quaternion rotation, Vector2 size, bool fill)
        {
            DrawDottedRect(center, rotation, size, defaultDottedScreenSpaceSize, fill);
        }

        /// <summary>
        /// Draws a dotted rect.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawDottedRect(Vector3 center, Quaternion rotation, Vector2 size, float screenSpaceSize = defaultDottedScreenSpaceSize, bool fill = false)
        {
#if UNITY_EDITOR
            DrawDottedRect(Handles.color, center, rotation, size, screenSpaceSize, fill);
#endif
        }

        /// <summary>
        /// Draws a dotted rect with the specified color.
        /// </summary>
        public static void DrawDottedRect(Color color, Vector3 center, Quaternion rotation, Vector2 size, bool fill)
        {
            DrawDottedRect(color, center, rotation, size, defaultDottedScreenSpaceSize, fill);
        }

        /// <summary>
        /// Draws a dotted rect with the specified color.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawDottedRect(Color color, Vector3 center, Quaternion rotation, Vector2 size, float screenSpaceSize = defaultDottedScreenSpaceSize, bool fill = false)
        {
#if UNITY_EDITOR
            // Cache
            Color startColor = Handles.color;
            Handles.color = color;
            Vector3[] corners = GetRectCorners(center, rotation, size);

            // Draw dotted rect
            Handles.DrawDottedLine(corners[0], corners[1], screenSpaceSize);
            Handles.DrawDottedLine(corners[1], corners[2], screenSpaceSize);
            Handles.DrawDottedLine(corners[2], corners[3], screenSpaceSize);
            Handles.DrawDottedLine(corners[3], corners[0], screenSpaceSize);

            // Reset handles color
            Handles.color = startColor;

            // Fill
            if (!fill)
                return;

            Handles.color = color.AlphaRelative(0.02f);
            Handles.DrawAAConvexPolygon(corners[0], corners[1], corners[2], corners[3]);

            Handles.color = startColor;
#endif
        }


        /// <summary>
        /// Draws a frustum rect.
        /// </summary>
        public static void DrawFrustumRect(Vector3 center, Quaternion rotation, float FOV, float aspect, float cameraDistance, bool fill)
        {
            DrawFrustumRect(center, rotation, FOV, aspect, cameraDistance, defaultDottedScreenSpaceSize, fill);
        }

        /// <summary>
        /// Draws a frustum rect.
        /// </summary>
        public static void DrawFrustumRect(Vector3 center, Quaternion rotation, Camera camera, float cameraDistance, bool fill)
        {
            DrawFrustumRect(center, rotation, camera.fieldOfView, camera.aspect, cameraDistance, defaultDottedScreenSpaceSize, fill);
        }

        /// <summary>
        /// Draws a frustum rect.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawFrustumRect(Vector3 center, Quaternion rotation, float FOV, float aspect, float cameraDistance, float lineWidth = defaultLineWidth, bool fill = false)
        {
#if UNITY_EDITOR
            DrawFrustumRect(Handles.color, center, rotation, FOV, aspect, cameraDistance, lineWidth, fill);
#endif
        }

        /// <summary>
        /// Draws a frustum rect.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawFrustumRect(Vector3 center, Quaternion rotation, Camera camera, float cameraDistance, float lineWidth = defaultLineWidth, bool fill = false)
        {
#if UNITY_EDITOR
            DrawFrustumRect(Handles.color, center, rotation, camera.fieldOfView, camera.aspect, cameraDistance, lineWidth, fill);
#endif
        }

        /// <summary>
        /// Draws a frustum rect with the given color.
        /// </summary>
        public static void DrawFrustumRect(Color color, Vector3 center, Quaternion rotation, float FOV, float aspect, float cameraDistance, bool fill)
        {
            DrawFrustumRect(color, center, rotation, FOV, aspect, cameraDistance, defaultDottedScreenSpaceSize, fill);
        }

        /// <summary>
        /// Draws a frustum rect with the given color.
        /// </summary>
        public static void DrawFrustumRect(Color color, Vector3 center, Quaternion rotation, Camera camera, float cameraDistance, bool fill)
        {
            DrawFrustumRect(color, center, rotation, camera.fieldOfView, camera.aspect, cameraDistance, defaultDottedScreenSpaceSize, fill);
        }

        /// <summary>
        /// Draws a frustum rect with the given color.
        /// </summary>
        public static void DrawFrustumRect(Color color, Vector3 center, Quaternion rotation, float FOV, float aspect, float cameraDistance, float lineWidth = defaultLineWidth, bool fill = false)
        {
            DrawRect(color, center, rotation, GetFrustumAtDistance(FOV, aspect, cameraDistance), lineWidth, fill);
        }

        /// <summary>
        /// Draws a rect.
        /// </summary>
        public static void DrawRect(Vector3 center, Quaternion rotation, Vector2 size, bool fill)
        {
            DrawRect(center, rotation, size, defaultDottedScreenSpaceSize, fill);
        }

        /// <summary>
        /// Draws a rect.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawRect(Vector3 center, Quaternion rotation, Vector2 size, float lineWidth = defaultLineWidth, bool fill = false)
        {
#if UNITY_EDITOR
            DrawRect(Handles.color, center, rotation, size, lineWidth, fill);
#endif
        }

        /// <summary>
        /// Draws a rect with the specified color.
        /// </summary>
        public static void DrawRect(Color color, Vector3 center, Quaternion rotation, Vector2 size, bool fill)
        {
            DrawRect(color, center, rotation, size, defaultDottedScreenSpaceSize, fill);
        }

        /// <summary>
        /// Draws a rect with the specified color.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawRect(Color color, Vector3 center, Quaternion rotation, Vector2 size, float lineWidth = defaultLineWidth, bool fill = false)
        {
#if UNITY_EDITOR
            // Cache
            Color startColor = Handles.color;
            Handles.color = color;
            if (lineWidth < 1f)
                lineWidth = 1f;
            Vector3[] corners = GetRectCorners(center, rotation, size);

            // Draw rect
            Handles.DrawAAPolyLine(lineWidth, corners[0], corners[1], corners[2], corners[3], corners[0]);

            // Reset handles color
            Handles.color = startColor;

            // Fill
            if (!fill)
                return;

            Handles.color = color.AlphaRelative(0.02f);
            Handles.DrawAAConvexPolygon(corners[0], corners[1], corners[2], corners[3]);

            Handles.color = startColor;
#endif
        }


        /// <summary>
        /// Draws a colored label.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Label(string label, Vector3 position, Color color)
        {
#if UNITY_EDITOR
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = color;
            labelStyle.richText = true;
            Handles.Label(position, label, labelStyle);
#endif
        }

        /// <summary>
        /// Draw a colored label that fades over distance.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Label(string label, Vector3 position, Color color, int maxFadeDistance, int minFadeDistance)
        {
#if UNITY_EDITOR
            float labelSize = HandleUtility.GetHandleSize(position);
            Label(label, position, color.Alpha(Mathf.InverseLerp(maxFadeDistance, minFadeDistance, labelSize)));
#endif
        }

#endregion

#region PRIVATE UTILS

        private static Vector3 ViewForward(Vector3 worldPoint)
        {
            return camera.orthographic ? view.forward : (view.position - worldPoint).normalized;
        }

        private static Quaternion GetDeformRotation(Vector3 point)
        {
            return Quaternion.FromToRotation(view.forward, (point - view.position).normalized);
        }

        private static List<Vector3> GetViewCircleVertices(Vector3 position, float radius, int subdivisions, bool closeShape = true, bool clockwise = false)
        {
            if (subdivisions % 2 > 0)
                subdivisions++;

            List<Vector3> vertices = new List<Vector3>();
            float angleStep = 360f / subdivisions;
            if (clockwise)
                angleStep *= -1;

            Quaternion fovDeformRot = camera.orthographic ? Quaternion.identity : GetDeformRotation(position);

            for (int i = 0; i < subdivisions; i++)
            {
                vertices.Add(position + fovDeformRot * Quaternion.AngleAxis(i * angleStep, view.forward) * view.right * radius);
            }

            if (closeShape)
                vertices.Add(vertices[0]);

            return vertices;
        }

        private static List<Vector3> GetArrowVertices(Vector3 startPosition, Vector3 endPosition, Vector3 facingNormal = default, float arrowSize = 0.5f)
        {
            // Cache
            List<Vector3> vertices = new List<Vector3>();
            Vector3 arrowVector = endPosition - startPosition;
            float arrowLength = arrowVector.magnitude;
            float stretchSize = Mathf.Clamp(arrowSize, 0f, arrowLength);
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, arrowVector.normalized);

            Matrix4x4 arrowMatrix = Matrix4x4.TRS(startPosition, rotation, Vector3.one);
            Vector2 matrixOffsetPosition = arrowMatrix.inverse.MultiplyPoint3x4(facingNormal != default ? startPosition + facingNormal : view.position);
            float angleOffset = Vector2.SignedAngle(Vector2.right, matrixOffsetPosition);
            Quaternion rotationOffset = Quaternion.Euler(0, 0, angleOffset + 90);

            if (arrowSize < 0f)
                arrowSize = 0f;

            vertices.Add(arrowMatrix.MultiplyPoint(rotationOffset * new Vector3(0.25f * arrowSize, 0, arrowLength - 0.5f * stretchSize)));
            vertices.Add(arrowMatrix.MultiplyPoint(rotationOffset * new Vector3(0.25f * arrowSize, 0)));
            vertices.Add(arrowMatrix.MultiplyPoint(rotationOffset * new Vector3(-0.25f * arrowSize, 0)));
            vertices.Add(arrowMatrix.MultiplyPoint(rotationOffset * new Vector3(-0.25f * arrowSize, 0, arrowLength - 0.5f * stretchSize)));
            vertices.Add(arrowMatrix.MultiplyPoint(rotationOffset * new Vector3(-0.5f * arrowSize, 0, arrowLength - 0.5f * stretchSize)));
            vertices.Add(arrowMatrix.MultiplyPoint(rotationOffset * new Vector3(0, 0, arrowLength)));
            vertices.Add(arrowMatrix.MultiplyPoint(rotationOffset * new Vector3(0.5f * arrowSize, 0, arrowLength - 0.5f * stretchSize)));

            vertices.Add(vertices[0]);

            return vertices;
        }

        private static List<Vector3> GetViewArrowVertices(Vector3 position1, Vector3 position2, float arrowSize = 0.5f)
        {
#if UNITY_EDITOR
            // Cache
            Vector3 viewVector1 = position1 - view.position;
            Vector3 viewVector2 = position2 - view.position;
            bool position1Nearest = viewVector1.magnitude < viewVector2.magnitude;
            Vector3 worldCenter = (position1 + position2) / 2f;

            // Flatten positions
            UnityEngine.Plane plane = new UnityEngine.Plane(view.forward, worldCenter);
            float enter = 0f;
            Vector3 hitPoint = Vector3.zero;

            Ray ray1 = new Ray(view.position, viewVector1.normalized);
            if (plane.Raycast(ray1, out enter))
            {
                position1 = ray1.GetPoint(enter);
            }

            Ray ray2 = new Ray(view.position, viewVector2.normalized);
            if (plane.Raycast(ray2, out enter))
            {
                position2 = ray2.GetPoint(enter);
            }

            Handles.SphereHandleCap(-1, position1, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.Label(position1, $"{1} / Distance: {Vector3.Distance(view.position, position1)}");

            Handles.SphereHandleCap(-1, position2, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.Label(position2, $"{2} / Distance: {Vector3.Distance(view.position, position2)}");

            Vector3 viewCenter = (position1 + position2) / 2f;
            Vector3 viewCenterVector = (viewCenter - view.position).normalized;
            float arrowLength = (position2 - position1).magnitude;
            float angle = Vector2.SignedAngle(Vector2.right, (camera.WorldToScreenPoint(position2) - camera.WorldToScreenPoint(position1)).normalized);

            // Creating arrow vertices
            List<Vector3> vertices = new List<Vector3>();
            Quaternion rotation = Quaternion.AngleAxis(angle, viewCenterVector);

            vertices.Add(viewCenter + rotation * view.rotation * new Vector3(0, -0.25f, 0) * arrowSize);
            vertices.Add(viewCenter + rotation * view.rotation  * new Vector3(-0.5f * arrowLength / arrowSize, -0.25f, 0) * arrowSize);
            vertices.Add(viewCenter + rotation * view.rotation * new Vector3(-0.5f * arrowLength / arrowSize, 0.25f, 0) * arrowSize);
            vertices.Add(viewCenter + rotation * view.rotation * new Vector3(0, 0.25f, 0) * arrowSize);
            vertices.Add(viewCenter + rotation * view.rotation * new Vector3(0, 0.5f, 0) * arrowSize);
            vertices.Add(viewCenter + rotation * view.rotation * new Vector3(0.5f, 0, 0) * arrowSize);
            vertices.Add(viewCenter + rotation * view.rotation * new Vector3(0, -0.5f, 0) * arrowSize);

            vertices.Add(vertices[0]);

            return vertices;
#else
            return new List<Vector3>();
#endif
        }

        private static List<Vector3> GetViewCapsuleVertices(Vector3 position1, float radius1, Vector3 position2, float radius2, int subdivisions)
        {
            // Cache
            if (subdivisions < 4)
                subdivisions = 4;
            else if (subdivisions % 2 > 0)
                subdivisions++;

            // Get view-facing circles
            List<Vector3> circle1 = GetViewCircleVertices(position1, radius1, subdivisions, false);
            List<Vector3> circle2 = GetViewCircleVertices(position2, radius2, subdivisions, false);

            // Output list
            List<Vector3> output = circle1;
            output.AddRange(circle2); // Combine both lists

            // ICI CHECK si la caméra est dans la capsule et afficher un gros gizmo qui occlue la caméra
            // If camera distance < position cercle avec radius
            // Ou que la cam est dans le cylindre décrit par les deux cercles

            // Convex hull algorithm
            output = Get2DConvexHullShape(output);

            return output;
        }

        private static List<Vector3> Get2DConvexHullShape(List<Vector3> vertices)
        {
            // Less than 4 vertices is already a convex shape
            if (vertices.Count < 4)
                return vertices;

            // Cache
            List<Vector3> convexShape = new List<Vector3>();

            // Sort vertices from X screen space position
            vertices = vertices.OrderBy(vertex => camera.WorldToScreenPoint(vertex).x).ToList();

            // Create view-facing vertices cloud for angle sampling purposes
            List<Vector2> facingVertices = new List<Vector2>();
            for (int vertexID = 0; vertexID < vertices.Count; vertexID++)
            {
                facingVertices.Add(camera.WorldToScreenPoint(vertices[vertexID]));
            }

            // Add the very first vertex
            convexShape.Add(vertices[0]);

            // Loop until the shape has been completely closed
            bool initialized = false;
            int previousConvexVertexID = -1;
            int lastConvexVertexID = 0;
            Vector2 lastReferenceVector = Vector2.right;
            int iterations = 0;
            while (!initialized || lastConvexVertexID != 0)
            {
                if (iterations > 2048) // Prevent infinite loop
                    break;

                initialized = true;

                float maxAngle = 0f;
                Vector2 cacheVector = Vector2.zero;
                int cacheVertex = lastConvexVertexID;

                for (int vertexID = 0; vertexID < facingVertices.Count; vertexID++)
                {
                    // Same vertex
                    if (vertexID == lastConvexVertexID || vertexID == previousConvexVertexID)
                        continue;

                    Vector2 vector = (facingVertices[vertexID] - facingVertices[lastConvexVertexID]).normalized;
                    float angle = Vector2.SignedAngle(lastReferenceVector, vector);

                    if (angle > maxAngle)
                    {
                        // Edit max angle
                        maxAngle = angle;

                        // Cache vertex info
                        cacheVertex = vertexID;
                        cacheVector = vector;
                    }

                    iterations++;
                }

                lastReferenceVector = -cacheVector;
                previousConvexVertexID = lastConvexVertexID;
                lastConvexVertexID = cacheVertex;

                convexShape.Add(vertices[cacheVertex]);
            }

            return convexShape;
        }

#endregion
    }
}
