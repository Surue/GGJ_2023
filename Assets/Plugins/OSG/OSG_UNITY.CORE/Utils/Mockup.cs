// Old Skull Games
// Flavien Caston
// Monday 1, July, 2019

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ShortcutManagement;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace OSG
{
    public class Mockup : MonoBehaviour
    {
        private SpriteRenderer _sprite;
        public SpriteRenderer sprite
        {
            get
            {
                if (!_sprite)
                    _sprite = GetComponent<SpriteRenderer>();

                return _sprite;
            }
            set { _sprite = value; }
        }

        private Image _image;
        public Image image
        {
            get
            {
                if (!_image)
                    _image = GetComponent<Image>();

                return _image;
            }
            set { _image = value; }
        }

        private float alpha;

        public static void IncreaseAlpha()
        {
            ForEach<Mockup>.Do(m => { m.IncrementAlphaBy(0.25f); });
        }

        public static void DecreaseAlpha()
        {
            ForEach<Mockup>.Do(m => { m.IncrementAlphaBy(-0.25f); });
        }

        public void IncrementAlphaBy(float value)
        {
            SetAlpha(alpha + value);
        }

        public void SetAlpha(float _alpha)
        {
            alpha = Mathf.Clamp01(_alpha);

            if (sprite)
                sprite.color = Color.white.Alpha(alpha);

            if (image)
                image.color = Color.white.Alpha(alpha);
        }

#if UNITY_EDITOR
        [Shortcut("ShowHideMockup")]
#endif
        public static void ShowHideMockup()
        {
            ForEach<Mockup>.Do(m => { m.gameObject.SetActive(!m.gameObject.activeSelf); });
        }
    }

#if UNITY_EDITOR
    public class MockupCreator
    {
        [MenuItem("GameObject/UI/Mockup", false, 55)]
        private static void CreateUIMockup()
        {
            var mockup = CreateMockup<Image>();
            mockup.transform.SetAsLastSibling();

            mockup.image.raycastTarget = false;
        }

        [MenuItem("GameObject/2D Object/Mockup", false, 55)]
        private static void CreateEnvironmentMockup()
        {
            CreateMockup<SpriteRenderer>();
        }

        private static Mockup CreateMockup<T>()
        {
            GameObject mockupGO = new GameObject("Mockup");
            Mockup mockup = mockupGO.AddComponent<Mockup>();

            mockupGO.AddComponent(typeof(T));
            if (Selection.activeGameObject != null)
                mockupGO.transform.SetParent(Selection.activeGameObject.transform);
            mockupGO.transform.Reset();

            mockup.SetAlpha(0.5f);

            EditorGUIUtility.PingObject(mockupGO);
            Selection.activeGameObject = mockupGO;

            return mockup;
        }
    }
#endif
}
