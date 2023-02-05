// Old Skull Games
// Bernard Barthelemy
// Tuesday, November 9, 2017


using UnityEngine;
using UnityEditor;
namespace OSG
{
    /// <summary>
    /// Prompt for a string with this dialog
    /// </summary>
    public class InputStringDialog : EditorWindow
    {
        public delegate bool OnAccept(string value);
        private OnAccept acceptAction;
        private string result; // final result to be validate
        private bool area;     
        private string typing;
        [EditorPrefs] private static bool controlEnterToValidate;

        /// <summary>
        /// Open the dialog
        /// </summary>
        /// <param name="onAccept">You'll get the result via this callback, accept by returning true</param>
        /// <param name="text">Dialog's title</param>
        /// <param name="textArea">true to accept multi line string</param>
        /// <param name="defaultValue">default value for the text parameter</param>
        public static InputStringDialog Open(OnAccept onAccept, string text, bool textArea, string defaultValue = "")
        {
            var w = ScriptableObject.CreateInstance<InputStringDialog>(); //GetWindow<WaitForKeyWindow>();
            w.titleContent.text = text;
            //w.ShowAsDropDown(r, r.size);
            w.acceptAction = onAccept;
            w.area = textArea;
            w.ShowUtility();
            w.minSize = new Vector2(100, 32);
            if (!string.IsNullOrEmpty(defaultValue))
                w.typing = defaultValue;
            return w;
        }
        //         TextEditor te = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

        public static void Open(Rect r,OnAccept onAccept, string text, bool textArea, string defaultValue = "")
        {
            var w = ScriptableObject.CreateInstance<InputStringDialog>(); //GetWindow<WaitForKeyWindow>();
            w.titleContent.text = text;
            w.acceptAction = onAccept;
            w.area = textArea;
            
            //w.ShowAsDropDown(r, r.size);
            w.ShowUtility();
            w.position = r;
            
            if (!string.IsNullOrEmpty(defaultValue))
                w.typing = defaultValue;
        }

        public static GUIStyle optionStyle;

        void OnGUI()
        {
            Focus();
            try
            {
                GUI.SetNextControlName("InputField");
                    
                typing = area ? EditorGUILayout.TextArea(typing) : EditorGUILayout.TextField(typing);
                
                Event current = Event.current;
                if (current == null) return;
                
                if (current.type == EventType.KeyUp)
                {
                    OnKeyUp(current);
                }
                if(area)
                {
                    GUILayout.BeginHorizontal();
                    if(GUILayout.Button("Ok"))
                    {
                        Validate();
                    }
                    optionStyle = optionStyle??new GUIStyle(GUI.skin.label)
                    {
                        normal = {textColor = Color.grey},
                        fontSize = 8,
                        fontStyle = FontStyle.Italic
                    };
                    GUIContent content = new GUIContent(controlEnterToValidate ? "CTRL-ENTER validates" : "CTRL-ENTER adds line");
                    Vector2 size = optionStyle.CalcSize(content);
                    if(GUILayout.Button(content, optionStyle, GUILayout.Width(size.x)))
                    {
                        controlEnterToValidate=!controlEnterToValidate;
                    }
                    
                    GUILayout.EndHorizontal();
                }

                EditorGUI.FocusTextInControl("InputField");
            }
            catch
            {
                Close();
            }
        }

        private void OnKeyUp(Event current)
        {
            //TextEditor te = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            
            if (current.keyCode == KeyCode.Escape)
                Close();
            else if (current.keyCode == KeyCode.Return)
            {
                if (area)
                {
                    if (controlEnterToValidate)
                    {
                        if (current.control)
                            Validate();
                    }
                    else if(!current.control)    
                        Validate();
                }
                else
                {
                    Validate();
                }
            }
            result = typing;
        }

        private void Validate()
        {
            if(acceptAction(result))
            {
                GUI.FocusControl(null);
                Close();
            }
        }
    }
}