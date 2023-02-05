// Old Skull Games
// Antoine Pastor
// 01/11/2018

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public static class GUIHelpers {
#if UNITY_EDITOR
	public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
	{
		Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
		r.height = thickness;
		r.y+=padding/2;
		r.x-=2;
		r.width +=6;
		EditorGUI.DrawRect(r, color);
	}

	public static void DrawTitle(string titleText)
	{
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(titleText, EditorStyles.boldLabel);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
#endif
}	



