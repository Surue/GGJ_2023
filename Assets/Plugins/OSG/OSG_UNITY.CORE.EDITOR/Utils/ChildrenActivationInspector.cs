using UnityEditor;
using UnityEngine;

namespace OSG
{
	[CustomEditor(typeof(ChildrenActivation), true)]
	public class ChildrenActivationInspector : CustomEditorBase
	{
		protected ChildrenActivation childrenActivation;

		protected override void OnEnable()
		{
            base.OnEnable();
			childrenActivation = target as ChildrenActivation;
		}
		

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (GUILayout.Button("Apply"))
			{
				
				for (int i = 0; i < childrenActivation.transformCached.childCount; i++)
				{
					Transform childTransform = childrenActivation.transformCached.GetChild(i).transform;
					string childName = childTransform.name;
					childTransform.gameObject.SetActive(childrenActivation.objectsToActivate.Contains(childName));
				}

			}
			for (int i= 0; i<childrenActivation.transformCached.childCount; i++)
			{
				string childName = childrenActivation.transformCached.GetChild(i).name;
				bool wasActive = childrenActivation.objectsToActivate.Contains(childName);
				bool isActive = EditorGUILayout.Toggle(childName, wasActive);

				if (wasActive != isActive)
				{
					if (isActive)
						childrenActivation.objectsToActivate.Add(childName);
					else
						childrenActivation.objectsToActivate.Remove(childName);
				}
			}
			
		}
	}
}
