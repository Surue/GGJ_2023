using UnityEngine;
using UnityEditor;

namespace OSG
{
    using static EnhancedEditor;

    [CustomEditor(typeof(BezierCurve))]
    public class BezierCurveEditor : Editor
    {
        private static string textContent =
            "\n<color=orange>➜</color> A BezierCurve can't have less than two anchor points" +
            "\n<color=orange>➜</color> The higher the segmentation value is, the more accurate is the curve" +
            "\n<color=orange>➜</color> A segmentation of 1 results in a linear curve" +
            "\n<color=orange>➜</color> Accuracy loss can occur when snapping tangents" +
            "\n<color=orange>➜</color> Even if BezierCurve editing works regardless of the transform matrix (rotation and scale), prefer keeping a clean one" +
            "\n" +
            "\n<color=orange>Drag</color>: Move anchor or tangent" +
            "\n<color=orange>Ctrl + Drag</color>: Snap anchor or tangent (snap value is editable in inspector window)" +
            "\n<color=orange>Alt + Drag</color>: Break and move tangent" +
            "\n" +
            "\n<color=orange>A</color>: Add new anchor point after the selected one" +
            "\n<color=orange>D</color>: Delete selected anchor point" +
            "\n<color=orange>G</color>: Reset selected anchor tangents" +
            "\n<color=orange>S</color>: Close/Open curve" +
            "\n<color=orange>C</color>: Reset curve (2 anchors)" +
            "\n";


        public override void OnInspectorGUI()
        {
            // Cache
            SerializedProperty curveAnchors = serializedObject.FindProperty("_curveAnchors");
            SerializedProperty closeCurve = serializedObject.FindProperty("_closeCurve");
            SerializedProperty segmentation = serializedObject.FindProperty("_segmentation");
            SerializedProperty maxCurveLength = serializedObject.FindProperty("_maxCurveLength");
            SerializedProperty curvePoints = serializedObject.FindProperty("_curvePoints");
            SerializedProperty axis = serializedObject.FindProperty("axis");
            SerializedProperty axisOffset = serializedObject.FindProperty("axisOffset");

            // GUI
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(curveAnchors, true);
                
                SmallSpace();

                GUILayout.BeginVertical("box");
                {
                    int maxSegmentation = (maxCurveLength.intValue - 1) / (closeCurve.boolValue ? curveAnchors.arraySize : curveAnchors.arraySize - 1);

                    GUI.enabled = maxSegmentation > 1;
                    GUILayout.BeginHorizontal();
                    {
                        segmentation.intValue = Mathf.Clamp(EditorGUILayout.IntSlider("Segmentation", segmentation.intValue, 1, maxSegmentation), 1, maxSegmentation); // Clamp is necessary in case the max curve length parameter has been edited
                        GUILayout.Label($"1 - {maxSegmentation}", EnhancedGUI.centeredTextStyle, GUILayout.Width(60));
                    }
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;

                    EditorGUILayout.PropertyField(maxCurveLength);
                    GUILayout.Label($"Current curve length: {curvePoints.arraySize}");

                    EditorGUILayout.PropertyField(closeCurve);

                    SmallSpace();

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("transformUpdate"));
                }
                GUILayout.EndVertical();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("drawCurveUnselected"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("curveColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("handlesColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedHandleColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ghostColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("snap"));

                GUI.enabled = !serializedObject.FindProperty("axisLock").boolValue;
                {
                    EditorGUILayout.PropertyField(axis);

                    if (axis.enumValueIndex > 0)
                    {
                        EditorGUILayout.PropertyField(axisOffset);

                        if (axisOffset.enumValueIndex > 0)
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("axisOffsetValue"));
                    }
                }
                GUI.enabled = true;
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }


        private void OnEnable()
        {
            (target as BezierCurve).lastHandleID = -1;
        }

        private void OnDisable()
        {
            (target as BezierCurve).lastHandleID = -1;
        }

        private void OnSceneGUI()
        {
            DrawInfo();
            DrawHandles();
            Shortcuts();
        }


        private void DrawHandles()
        {
            // Cache
            Event e = Event.current;
            BezierCurve curve = target as BezierCurve;

            // Draw handles
            EditorGUI.BeginChangeCheck();
            {
                int anchorsCount = curve.curveAnchors.Count;
                for (int i = 0; i < anchorsCount; i++)
                {
                    // Record undo
                    Undo.RecordObject(curve, "BezierCurve handle edit.");

                    // Anchor cache
                    BezierAnchor bezierAnchor = curve.curveAnchors[i];
                    Vector3 anchorPos = curve.transformCached.TransformPoint(bezierAnchor.anchor);
                    Vector3 inHandlePos = curve.transformCached.TransformPoint(bezierAnchor.anchor + bezierAnchor.inTangent);
                    Vector3 outHandlePos = curve.transformCached.TransformPoint(bezierAnchor.anchor + bezierAnchor.outTangent);
                    bool isCurrentAnchor = curve.lastHandleID == i;
                    bool isFirstHandle = i == 0;
                    bool isLastHandle = i == curve.curveAnchors.Count - 1;

                    bool closeHandle = isCurrentAnchor;
                    if (!isCurrentAnchor)
                    {
                        if (curve.lastHandleID == 0)
                            closeHandle = i == 1 || i == curve.curveAnchors.Count - 1;
                        else if (curve.lastHandleID == curve.curveAnchors.Count - 1)
                            closeHandle = i == 0 || i == curve.curveAnchors.Count - 2;
                        else
                            closeHandle = curve.lastHandleID > -1 && Mathf.Abs(curve.lastHandleID - i) < 2;
                    }

                    float handle1Distance = Vector3.Distance(anchorPos, inHandlePos);
                    float handle2Distance = Vector3.Distance(anchorPos, outHandlePos);
                    float anchorDistance = handle1Distance < handle2Distance ? handle1Distance : handle2Distance;
                    float anchorSize = Mathf.Clamp(HandleUtility.GetHandleSize(anchorPos) * 0.1f, 0, closeHandle ? anchorDistance * 0.5f : Mathf.Infinity);
                    float handle1Size = Mathf.Clamp(HandleUtility.GetHandleSize(inHandlePos) * 0.1f, 0, closeHandle ? handle1Distance * 0.5f : Mathf.Infinity);
                    float handle2Size = Mathf.Clamp(HandleUtility.GetHandleSize(outHandlePos) * 0.1f, 0, closeHandle ? handle2Distance * 0.5f : Mathf.Infinity);
                    Color color = isCurrentAnchor ? curve.selectedHandleColor : curve.handlesColor;
                    Color alphaColor = color.AlphaRelative(0.25f);
                    Color anchorColor = closeHandle ? color : alphaColor;
                    Color leftHandleColor = i == curve.lastHandleID - 1 ? alphaColor : color;
                    Color rightHandleColor = i == curve.lastHandleID + 1 ? alphaColor : color;
                    bool linear = curve.segmentation == 1;
                    Vector3 axisOffsetPoint = curve.GetAxisOffsetPoint();

                    // Anchor handle
                    int handleID = -1;

                    Handles.color = anchorColor;
                    EditorGUI.BeginChangeCheck();
                    Vector3 newAnchor = Handles.FreeMoveHandle(i + 1, anchorPos, Quaternion.identity, anchorSize, Vector3.zero, Handles.CircleHandleCap);
                    if (EditorGUI.EndChangeCheck())
                        handleID = 0;

                    if (e.control && handleID == 0)
                        newAnchor = newAnchor.Snap(curve.snap);

                    curve.FlattenPoint(ref newAnchor, axisOffsetPoint);

                    bezierAnchor.anchor = curve.transformCached.InverseTransformPoint(newAnchor);

                    if (e.type == EventType.Used && GUIUtility.hotControl == i + 1)
                        curve.lastHandleID = i;

                    // In tangent handle
                    Handles.color = leftHandleColor;
                    EditorGUI.BeginChangeCheck();
                    Vector3 newInTangent = (closeHandle && !linear) ?
                        Handles.FreeMoveHandle(inHandlePos, Quaternion.identity, handle1Size, Vector3.zero, Handles.RectangleHandleCap) : inHandlePos;
                    if (EditorGUI.EndChangeCheck())
                        handleID = 1;

                    if (e.control && handleID == 1)
                        newInTangent = newInTangent.Snap(curve.snap);

                    curve.FlattenPoint(ref newInTangent, newAnchor);

                    if (closeHandle && !linear)
                        newInTangent = curve.transformCached.InverseTransformPoint(newInTangent) - bezierAnchor.anchor;

                    // Out tangent handle
                    Handles.color = rightHandleColor;
                    EditorGUI.BeginChangeCheck();
                    Vector3 newOutTangent = (closeHandle && !linear) ?
                        Handles.FreeMoveHandle(outHandlePos, Quaternion.identity, handle2Size, Vector3.zero, Handles.RectangleHandleCap) : outHandlePos;
                    if (EditorGUI.EndChangeCheck())
                        handleID = 2;

                    if (e.control && handleID == 2)
                        newOutTangent = newOutTangent.Snap(curve.snap);

                    curve.FlattenPoint(ref newOutTangent, newAnchor);

                    if (closeHandle && !linear)
                        newOutTangent = curve.transformCached.InverseTransformPoint(newOutTangent) - bezierAnchor.anchor;

                    // Non-editable handles
                    Handles.color = anchorColor;
                    Handles.SphereHandleCap(-1, anchorPos, Quaternion.identity, anchorSize, EventType.Repaint);

                    if (closeHandle)
                    {
                        if (!linear)
                        {
                            Handles.color = leftHandleColor;
                            Handles.SphereHandleCap(-1, inHandlePos, Quaternion.identity, handle1Size, EventType.Repaint);
                            Handles.DrawDottedLine(inHandlePos, anchorPos, 2);

                            Handles.color = rightHandleColor;
                            Handles.SphereHandleCap(-1, outHandlePos, Quaternion.identity, handle2Size, EventType.Repaint);
                            Handles.DrawDottedLine(outHandlePos, anchorPos, 2);
                        }

                        Handles.Label(anchorPos, $"<color=#{ColorUtility.ToHtmlStringRGBA(anchorColor)}>#{i.ToString("000")}</color>", EnhancedGUI.richText);
                    }

                    // No handle has been moved
                    if (handleID == -1)
                    {
                        Undo.FlushUndoRecordObjects();
                        continue;
                    }

                    if (handleID > 0)
                    {
                        bezierAnchor.inTangent = e.alt ? newInTangent : -newOutTangent;
                        bezierAnchor.outTangent = e.alt ? newOutTangent : -newInTangent;
                    }

                    if (curve.closeCurve || !isLastHandle)
                        curve.CacheSegmentPoints(i);

                    if (curve.closeCurve)
                        curve.CacheSegmentPoints(i == 0 ? anchorsCount - 1 : i - 1);
                    else if (!isFirstHandle)
                        curve.CacheSegmentPoints(i - 1);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(curve);
            }
        }

        private void DrawInfo()
        {
            Handles.BeginGUI();

            GUI.color = UnityEditorInternal.InternalEditorUtility.HasPro() ? Color.black : new Color(0.1f, 0.1f, 0.1f, 0.25f);
            GUILayout.BeginVertical("box", GUILayout.Width(320));
            {
                GUILayout.Label(
                    "<color=#E6E6E6><b>BezierCurve tooltips & shortcuts</b>\n" +
                    (BezierCurve.showTooltips ? textContent : "") +
                    $"\n<color=orange>Escape</color>: {(BezierCurve.showTooltips ? "Collapse" : "Expand")} tooltips</color>",
                    EnhancedGUI.richLabelWrapStyle);
            }
            GUILayout.EndVertical();

            Handles.EndGUI();
        }

        private void Shortcuts()
        {
            // Cache
            Event e = Event.current;
            BezierCurve curve = target as BezierCurve;

            // Shortcuts
            if (e == null)
                return;

            // Prevent triggering another editor shortcut
            if (e.control || e.shift || e.alt)
                return;

            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    case KeyCode.A:
                        curve.AddAnchorAfterSelection();
                        break;

                    case KeyCode.D:
                        curve.DeleteSelectedAnchor();
                        break;

                    case KeyCode.G:
                        curve.ResetSelectedAnchorTangent();
                        break;

                    case KeyCode.S:
                        curve.CloseCurve(!curve.closeCurve);
                        break;

                    case KeyCode.C:
                        curve.ResetCurve();
                        break;

                    case KeyCode.Escape:
                        BezierCurve.showTooltips = !BezierCurve.showTooltips;
                        break;
                }
            }
        }
    }
}