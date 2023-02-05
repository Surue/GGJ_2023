// Old Skull Games
// Benoit Constantin
// Monday, February 25, 2019

using UnityEngine;
using UnityEditor;

namespace OSG
{
    public class GraphTestWindow : EditorWindow
    {
        //// Add menu named "My Window" to the Window menu
        //[MenuItem("Test/Graph")]
        //static void Init()
        //{
        //    // Get existing open window or if none, make a new one:
        //    GraphTestWindow window = (GraphTestWindow)EditorWindow.GetWindow(typeof(GraphTestWindow));

        //    window.Show();
        //}


        EditorBorderedFunctionalCurveDrawer curveDrawer;
        private void OnEnable()
        {
            if (curveDrawer == null)
            {
                curveDrawer = new EditorBorderedFunctionalCurveDrawer();
                curveDrawer.F = Mathf.Sin;
                curveDrawer.title = "SinX/X";
                curveDrawer.xAxisName = "X";
                curveDrawer.yAxisName = "Sin";
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.FlexibleSpace();

                curveDrawer.OnGUI();

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();

            if (GUI.changed)
                Repaint();
        }
    }
}
