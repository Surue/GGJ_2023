using UnityEditor;

namespace OSG
{
    [CustomEditor(typeof(EditorShortcutList))]
    public class EditorShortcutListEditor : CustomEditorBase
    {
        private EditorShortcutList list;

        protected override void OnEnable()
        {
            list = target as EditorShortcutList;
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(list) list.OnInspectorGUI();
        }
    }
}