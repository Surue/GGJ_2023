using UnityEngine;
public class Sample : MonoBehaviour
{
    // Style used for result text
    GUIStyle textStyle;

    // Convert number formats
    bool convertNumbers = true;

    // Treat whole text as a left to right text and do not change the format (use for rtl and ltr mixed texts)
    bool isLtrText = false;

    // Keep left to right words clean(even in Persian/Arabic texts)
    bool keepLtrClean = true;

    // Simulating right to left word wrap and line feeding properly based on bias
    int wordWrapBias = 49;

    //Init text => You can change this property in unity Inspector.
    //string text = "سلام، خوش آمدید" + "\n" + "السلام عیلکم" + "\n" + "ہیلو اردو" + "\n" + "Hello" + "\n" + "Bonjour" + "\n" + "1234567890";
    string text = "السلام عیلکم - سلام - ہیلو - Bonjour - Hello ";

    // Custom right to left text field skin
    public GUISkin skin;

    // Some sample fonts, you can easily import your desire fonts.
    public Font[] fonts;

    void Awake()
    {
        textStyle = new GUIStyle();
        textStyle.alignment = TextAnchor.UpperRight;
        textStyle.font = fonts[0];
        textStyle.normal.textColor = Color.white;
        textStyle.fontStyle = FontStyle.Bold;
    }
#if ENABLE_ONGUI
    void OnGUI()
    {
        GUI.skin = skin;

        // Draw font selection panel on top
        DrawFontSelectionPanel();

        // Draw text fields
        DrawTextPanel();

        // Draw email and contact information
        DrawInformation();
    }
#endif

    private void DrawFontSelectionPanel()
    {
        // Tahoma font is the default for this demo
        //skin.label.font = fonts[0];

        GUILayout.Space(10);

        GUILayout.Label("Version: 3.8 - Arabic, Hebrew, Kurdi, Persian, Urdu, Afghan right to left languages full support!");
    }
    private void DrawTextPanel()
    {
        convertNumbers = GUILayout.Toggle(convertNumbers, " Right to left number format");
        keepLtrClean = GUILayout.Toggle(keepLtrClean, " Keep left to right words neat and clean");
        isLtrText = GUILayout.Toggle(isLtrText, " It's an English text including foreign words within");

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Word Wrap Bias: ", GUILayout.Width(120));
        int.TryParse(GUILayout.TextArea(wordWrapBias.ToString(), GUILayout.Width(50), GUILayout.Height(25)), out wordWrapBias);
        //
        GUILayout.Space(100);
        GUILayout.Label("Fonts:");

        // Some famous English fonts. They also support Persian/Arabic texts
        if (GUILayout.Button("Tahoma", GUILayout.Width(80)))
        {
            textStyle.font = fonts[0];
        }
        if (GUILayout.Button("Time New Roman", GUILayout.Width(120)))
        {
            textStyle.font = fonts[1];
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(2);
        GUILayout.Box("", GUILayout.Width(Screen.width - 8), GUILayout.Height(10));

        GUILayout.BeginHorizontal();
        GUILayout.Space(5);
        //GUILayout.Label("Unity default, Enter your sample text:", GUILayout.Width(250));
        GUILayout.Label("Unity Default Text:", GUILayout.Width(150));
        // Normal unity default text
        text = GUILayout.TextArea(text, GUILayout.Height(50), GUILayout.Width(350));
        //GUILayout.Space(32);
        GUILayout.EndHorizontal();

        //Set selected font

        //GUI.color = new Color(0.95f, 0.2f, 0.2f);
        ////Default unity text
        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Unity Default Result:", GUILayout.Width(150));
        //GUILayout.Label(text, textStyle, GUILayout.Height(50), GUILayout.Width(350));
        //GUILayout.EndHorizontal();

        //GUI.color = Color.green;
        //RTL converted text
        GUILayout.BeginHorizontal();
        GUILayout.Label("RTL Converter Result:", GUILayout.Width(150));
        GUILayout.Label(RTL.GetText(text, convertNumbers, isLtrText, wordWrapBias, keepLtrClean), textStyle,
            GUILayout.Height(50), GUILayout.Width(350));
        GUILayout.EndHorizontal();

       //GUI.color = Color.white;
    }
    private void DrawInformation()
    {
        // Tahoma font is the default for this demo
        //skin.label.font = fonts[0];

        GUILayout.Space(6);
        GUILayout.Label("If you have any question don't hesitate to ask us: [ info@heygamers.com ]");
    }
}
