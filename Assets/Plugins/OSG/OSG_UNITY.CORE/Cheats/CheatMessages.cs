using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace OSG
{
    public static class CheatMessages
    {
        [EditorPrefs] public static bool enabled = true;
        public enum Position
        {
            Top,
            Bottom,
            Left,
            Right,
            Center
        }

        private static float messageDuration = 5;

        private class CheatMessage
        {
            private readonly GUIContent text;
            private float lifeTime;

            public CheatMessage(string message, float duration)
            {
                lifeTime = Time.realtimeSinceStartup + duration;
                text = new GUIContent(message);
            }

            public void Append(string msg)
            {
                text.text += msg;
            }

            public void OnGUI(ref Rect rect, GUIStyle style)
            {
                Vector2 size = style.CalcSize(text);
                rect.height = size.y;
                rect.y -= rect.height;
                GUI.Label(rect, text, style);
            }

            public bool IsDead => lifeTime<Time.realtimeSinceStartup;
        }

        private static readonly List<CheatMessage> messages = new List<CheatMessage>(10);

        public static void Display(string message, float duration = 0)
        {
            if (!enabled)
                return;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(duration == 0)
                duration = messageDuration;
            messages.Add(new CheatMessage(message, duration));
        }

        public static void Show(string message, Position position)
        {
            if (!enabled)
                return;
            debugMessages.Add(new DebugMessage(message, position));
        }

        struct DebugMessage
        {
            private readonly GUIContent text;
            #pragma warning disable 414
            [UsedImplicitly] Position position;

            public DebugMessage(string message, Position position)
            {
                this.position = position;
                text = new GUIContent(message);
            }

            public void Append(string msg)
            {
                text.text += msg;
            }

            public void OnGUI(GUIStyle style)
            {
                //Vector2 size = style.CalcSize(text);
                //var r = new Rect(0, Screen.height - 50, 100, 50);
                var r = new Rect(0,575, 373, 22);
                GUI.Label(r, text, style);
            }
        }
        private static List<DebugMessage> debugMessages = new List<DebugMessage>(10);

        public static void Append(string addendum)
        {
            if (messages.Count == 0)
            {
                Display(addendum);
            }
            else
            {
                messages[messages.Count-1].Append(" " + addendum);
            }
        }

        private static GUIContent clear;
        public static void OnGUI()
        {
            if (messages.Count == 0 && debugMessages.Count == 0)
                return;

            Rect r = new Rect(0, Screen.height, Screen.width, 2);

            
            
            for(int i = messages.Count;--i>=0;)
            {
                var message = messages[i];
                if(r.y >= 0)
                {
                    message.OnGUI(ref r, MessageStyle);
                }
                if (message.IsDead)
                {
                    messages.RemoveAt(i);
                }
            }



            for (int i = debugMessages.Count; --i >= 0;)
            {
                debugMessages[i].OnGUI(MessageStyle);
            }
            debugMessages.Clear();

            clear = clear ?? new GUIContent("X");
            Vector2 size = ButtonStyle.CalcSize(clear);

            r.xMin = r.xMax - size.y; // we want a square
            r.height = size.y;
            r.y = Mathf.Max(r.y, 0);
            if (GUI.Button(r, clear, ButtonStyle))
            {
                messages.Clear();
            }

        }

        private static GUIStyle _messageStyle;
        private static GUIStyle MessageStyle
        {
            get
            {
                return _messageStyle ??(_messageStyle =  new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    richText = true,
                    normal = {textColor = Color.white, background = BackgroundTexture}
                });
            }
        }

        private static GUIStyle _buttonStyle;

        private static GUIStyle ButtonStyle
        {
            get
            {
                return _buttonStyle ?? (_buttonStyle = new GUIStyle("SmallButton")
                {
                    fontSize = 20,
                    richText = true,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = {textColor = Color.red, background = BackgroundTexture}
                });
            }
        }



        private static Texture2D _background;
        private static Texture2D BackgroundTexture
        {
            get
            {
                if (_background) return _background;
                _background=new Texture2D(1,1);
                _background.SetPixels(new[]{new Color(0f,0f,0f,0.25f) });
                _background.Apply();
                return _background;
            }
        }
    }
}