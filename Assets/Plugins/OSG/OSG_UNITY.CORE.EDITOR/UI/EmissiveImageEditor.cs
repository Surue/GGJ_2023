using UnityEditor;

namespace UnityEngine.UI.Extensions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(EmissiveImage))]
    public class EmissiveImageEditor : Editor
    {
        private enum FillOrigin
        {
            Bottom,
            Right,
            Top,
            Left
        }

        private string emissiveMaterialPath = "Assets/OSG/Materials/MAT_UI_Default_Emissive.mat";
        private Material emissiveMaterial;

        public override void OnInspectorGUI()
        {
            // Check if material is not assigned
            if (!emissiveMaterial)
            {
                emissiveMaterial = AssetDatabase.LoadAssetAtPath(emissiveMaterialPath, typeof(Material)) as Material;
            }

            EmissiveImage image = (EmissiveImage)target;

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Source Image");
                image.sprite = EditorGUILayout.ObjectField(image.sprite, typeof(Sprite), false) as Sprite;
            }
            GUILayout.EndHorizontal();

            image.color = EditorGUILayout.ColorField("Color", image.color);

            GUI.enabled = emissiveMaterial;
            EditorGUI.BeginChangeCheck();
            {
                image.emissionColor = EditorGUILayout.ColorField(new GUIContent() { text = "Emission Color" }, image.emissionColor, true, false, false);
                image.emissionIntensity = EditorGUILayout.Slider("Emission Intensity", image.emissionIntensity, 0f, 1f);
            }
            if (EditorGUI.EndChangeCheck())
            {
                image.SetVerticesDirty();
            }
            GUI.enabled = true;

            // Material
            GUI.color = emissiveMaterial ? Color.white : Color.red;
            if (!emissiveMaterial)
                GUILayout.BeginVertical("box");
            GUI.color = Color.white;
            GUI.enabled = false; // Material is set automatically and cannot be changed
            image.material = emissiveMaterial ?? null;
            image.material = EditorGUILayout.ObjectField("Material", image.material, typeof(Material), false) as Material;
            if (!emissiveMaterial)
            {
                GUI.color = new Color(1, 0.5f, 0.5f, 1);
                GUI.skin.label.wordWrap = true;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.enabled = true;
                GUILayout.Label("Material \"MAT_UI_Default_Emissive\" not found in folder Assets/OSG/Materials");
                GUILayout.EndVertical();
            }
            GUI.enabled = true;
            GUI.color = Color.white;

            image.raycastTarget = EditorGUILayout.Toggle("Raycast Target", image.raycastTarget);

            GUILayout.Space(6);

            GUILayout.BeginVertical("box");
            {
                image.type = (Image.Type)EditorGUILayout.EnumPopup("Image Type", image.type);

                GUILayout.Space(6);

                switch (image.type)
                {
                    case Image.Type.Simple:
                        GUILayout.BeginHorizontal();
                        {
                            image.preserveAspect = EditorGUILayout.Toggle("Preserve Aspect", image.preserveAspect);
                            if (GUILayout.Button("Set Native Size"))
                            {
                                image.SetNativeSize();
                            }
                        }
                        GUILayout.EndHorizontal();
                        break;

                    case Image.Type.Sliced:
                        image.fillCenter = EditorGUILayout.Toggle("Fill Center", image.fillCenter);
                        break;

                    case Image.Type.Tiled:
                        image.fillCenter = EditorGUILayout.Toggle("Fill Center", image.fillCenter);
                        break;

                    case Image.Type.Filled:
                        image.fillMethod = (Image.FillMethod)EditorGUILayout.EnumPopup("Fill Method", image.fillMethod);
                        FillOrigin fillOrigin = FillOrigin.Bottom;
                        fillOrigin = (FillOrigin)EditorGUILayout.EnumPopup("Fill Origin", (FillOrigin)image.fillOrigin);
                        image.fillOrigin = (int)fillOrigin;
                        image.fillAmount = EditorGUILayout.Slider("Fill Amount", image.fillAmount, 0f, 1f);
                        image.fillClockwise = EditorGUILayout.Toggle("Clockwise", image.fillClockwise);
                        image.preserveAspect = EditorGUILayout.Toggle("Preserve Aspect", image.preserveAspect);
                        break;
                }
            }
        }

        //[MenuItem("GameObject/UI/Emissive Image")]
        static void FonctionQuiInstancieUnGameObjectAvecUnComponentCustomImage()
        {
            GameObject emissiveImageGo = new GameObject("Emissive Image", typeof(RectTransform));
            emissiveImageGo.transform.SetParent(Selection.GetTransforms(SelectionMode.TopLevel)[0]);
            emissiveImageGo.transform.localPosition = Vector3.zero;
            emissiveImageGo.transform.localScale = Vector3.one;
            emissiveImageGo.AddComponent<EmissiveImage>();
            emissiveImageGo.layer = 5;

            Selection.SetActiveObjectWithContext(emissiveImageGo, null);
        }
    }
}