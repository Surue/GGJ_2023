// Old Skull Games
// Bernard Barthelemy
// Monday, February 11, 2019

using System.IO;
using OSG.Properties;

using UnityEditor;
using UnityEngine;

namespace OSG
{
    [CustomEditor(typeof(Colorizer))]
    class ColorizerEditor : Editor
    {
        private PropertyFrontend<Texture2D> grayScale;
        private PropertyFrontend<Texture2D> colorized;
        private IntPropertyFrontend colorRange;
        private PropertyFrontend<Material> baseMaterial;
        private PropertyFrontend<Material> colorizeMaterial;
        private PropertyFrontend<Material> comparisonMaterial;
        private ColorPropertyFrontend color;


        private Texture2D preview;

        void OnEnable()
        {
            grayScale = new ObjectPropertyFrontend<Texture2D>(nameof(grayScale), this);
            colorized = new ObjectPropertyFrontend<Texture2D>(nameof(colorized), this);
            colorRange = new IntPropertyFrontend(nameof(colorRange), this);
            baseMaterial = new ObjectPropertyFrontend<Material>(nameof(baseMaterial), this);
            colorizeMaterial = new ObjectPropertyFrontend<Material>(nameof(colorizeMaterial), this);
            comparisonMaterial = new ObjectPropertyFrontend<Material>(nameof(comparisonMaterial), this);
            color = new ColorPropertyFrontend(nameof(color), this);
        }

        private struct MT
        {
            public string title;
            public Material m;
            public Texture2D t;

            public MT(Material m, Texture2D t, string title = null)
            {
                this.m = m;
                this.t = t;
                this.title = title;
            }
        }

        private static void DrawTextures(ref Rect previewArea, params MT[] ts)
        {
            Rect r = new Rect(previewArea);
            foreach (var pair in ts)
            {
                Texture2D t = pair.t;
                Material m = pair.m;
                if (!(t && m))
                    continue;

                int width = Mathf.Min((int) previewArea.width, t.width);
                float ratio = t.height;
                ratio /= t.width;

                r.width = width;
                r.height = width * ratio;

                if (r.xMax > previewArea.xMax)
                {
                    r.x = 0;
                    r.y += t.height;
                }

                EditorGUI.DrawPreviewTexture(r, t, m);
                if (!string.IsNullOrEmpty(pair.title))
                {
                    Rect titleRect = r;
                    titleRect.yMin = titleRect.yMax - 20;
                    GUI.Label(titleRect, pair.title,
                        new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter});
                }


                r.x += width;
                previewArea.yMin = r.yMax;
            }
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            string error = ReportErrors();
            if (error != null)
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
                return;
            }
            else if (GUILayout.Button("Analyze"))
            {
                Analyze();
            }

            if (preview && GUILayout.Button("Save texture"))
            {
                SavePreview();
            }

            Rect r = GUILayoutUtility.GetLastRect();
            r.yMin += 16;
            Material mat = colorizeMaterial;
            if (mat)
            {
                mat.SetTexture("_MainTex", preview);
                mat.SetColor("_Color", color);
            }

            mat = comparisonMaterial;
            if (mat)
            {
                mat.SetColor("_Color", color);
            }

            DrawTextures(ref r, new MT(baseMaterial, grayScale, "Gray"),
                new MT(comparisonMaterial, grayScale, "Multiply"),
                new MT(colorizeMaterial, preview, "Colorize"));
        }

        private void SavePreview()
        {
            byte[] png = preview.EncodeToPNG();
            string assetPath = AssetDatabase.GetAssetPath(grayScale.Get()).Substring("Assets/".Length)
                .Replace(".png", "_Colorizable.png");
            string path = Path.Combine(Application.dataPath, assetPath);
            File.WriteAllBytes(path, png);
        }

        private string ReportErrors()
        {
            try
            {
                Texture2D g = grayScale;
                if (!g)
                    return "No grayscale";

#if UNITY_2018_3_OR_NEWER
                if(!g.isReadable)
                    return $"{g.name} is not readable";
#else
                
                // THIS is supposed to throw an exception when a texture isn't readable, that's its only purpose
                #pragma  warning disable 219
                // ReSharper disable once NotAccessedVariable
                float r = g.GetPixel(0, 0).r;
#endif
                Texture2D c = colorized;
                if (!c)
                    return "No colorized";
#if UNITY_2018_3_OR_NEWER
                if(!c.isReadable)
                    return $"{c.name} is not readable";
#else
                r = c.GetPixel(0, 0).r;
                #pragma  warning restore 219
#endif
                if (c.width != g.width || c.height != g.height)
                    return $"Textures are not the same size {c.width}x{c.height} and {g.width}x{g.height}";

                return null;
            }
            catch (System.Exception e)
            {
                return e.Message;
            }
        }


        private Texture2D CreateEmptyTexture(Texture2D baseImage)
        {
            Texture2D t = new Texture2D(baseImage.width, baseImage.height, TextureFormat.RGBA32, false, true);
            byte[] rawTextureData = t.GetRawTextureData();
            for (var index = 0; index < rawTextureData.Length; index++)
            {
                rawTextureData[index] = 0;
            }

            t.LoadRawTextureData(rawTextureData);
            t.Apply();

            return t;
        }

        private void Analyze()
        {
            Texture2D gT = grayScale;
            Texture2D cT = colorized;
            preview = CreateEmptyTexture(gT);


            int with = gT.width;
            int height = gT.height;

            for (int x = 0; x < with; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float r = gT.GetPixel(x, y).r;
                    Color col = cT.GetPixel(x, y);
                    col.r -= r;
                    col.g -= r;
                    col.b -= r;
                    float g = Mathf.Sqrt(col.r * col.r + col.g * col.g + col.b * col.b);
                    Color cc = new Color(r, g, 0, col.a);
                    preview.SetPixel(x, y, cc);
                }
            }

            preview.Apply();
        }
    }
}