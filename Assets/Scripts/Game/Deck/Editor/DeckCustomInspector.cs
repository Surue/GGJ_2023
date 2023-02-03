using UnityEditor;

[CustomEditor(typeof(DeckScriptable))]
public class DeckCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var deckScriptable = (DeckScriptable)target;

        DrawDefaultInspector();
        
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LabelField("Card quantity", deckScriptable.GetTotalNumberOfCard().ToString());
        EditorGUI.EndDisabledGroup();
    }
}
