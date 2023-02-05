using OSG.DebugTools;
using UnityEditor;

namespace OSG
{
    [CustomEditor(typeof (DebugHistory))]
    public class DebugHistoryInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DebugHistory myTarget = (DebugHistory) target;
            base.OnInspectorGUI();
            myTarget.OnInspectorGUI(serializedObject);
        }
    }
}
