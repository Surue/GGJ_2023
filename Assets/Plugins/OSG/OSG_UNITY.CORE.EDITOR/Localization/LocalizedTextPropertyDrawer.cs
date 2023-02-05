using UnityEditor;
using UnityEngine;


namespace OSG
{
	[CustomPropertyDrawer(typeof(LocalizedTextAttribute))]
	public class LocalizedTextPropertyDrawer : PropertyDrawer
	{
		private const float Height = 32;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return Height ;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.height = Height;
			Rect topRect = position;
			topRect.height /= 2;
			topRect.y = position.y;
			
			Rect bottomRect = position;
			bottomRect.height /= 2;
			bottomRect.y = position.y + Height / 2;

            EditorGUI.PropertyField(topRect,property, new GUIContent(property.displayName+ " (Localized)"));
			string localizedString = Localization.Localize(property.stringValue);

			var boldtext = new GUIStyle(GUI.skin.label);
			boldtext.font = EditorStyles.boldFont;
			boldtext.fontSize = 10;

			if (property.stringValue == localizedString)
				GUI.contentColor = Color.red;

			EditorGUI.LabelField(bottomRect,
		        property.stringValue == localizedString ? "No key in localization file" : localizedString, boldtext);

            Rect r = bottomRect;
            r.xMin = r.xMax-30;
            r.yMin = r.yMax-16;
            if (GUI.Button(r, "...", GUI.skin.label))
            {
                LocalizationSelectorWindow.Choose(property);
            }

			GUI.contentColor = Color.white;

		}
	}
}