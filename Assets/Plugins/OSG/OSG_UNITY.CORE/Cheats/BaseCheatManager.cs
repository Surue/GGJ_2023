// Old Skull Games
// Bernard Barthelemy
// Thursday, July 6, 2017

#if UNITY_EDITOR || !DISABLE_CHEAT_MANAGER
#define CHEATS_ENABLED
#endif

#if CHEAT_MANAGER
#error Don't use  #define CHEAT_MANAGER, it's obsolete. use  #define DISABLE_CHEAT_MANAGER if you don't want the cheats
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OSG.Core.EventSystem;
using UnityEngine;


namespace OSG
{
    public partial class BaseCheatManager : OSGMono
    {
        private bool touchedDown;
        private CheatShortcut[] cheatShortcuts;
        private List<CheatMethodName> cheatsToHide;
        private Vector2 scrollPos;
        private static Texture2D myBlackTexture;
        private static GUIStyle _buttonStyle;
        private static GUISkin guiSkin;
        public Vector3 menuItemSizeInInch;
        private Vector3 unscaledItemSize;
        private bool show;

        // ReSharper disable once UnusedMember.Local
        private string cheatProfileName => RemoteSettings.GetString(nameof(cheatProfileName), "Adm1nCh34T");

        protected SavedFloat menuScale = new SavedFloat("CheatMenuScale",1f);

        /// <summary>
        /// To know if the CheatManager is actively listening to the user inputs.
        /// </summary>
        public bool IsListeningInputs { get; private set; }

#if CHEATS_ENABLED
        public bool CanCheat => true;
#else
        private static bool supercheatActivated;
        public bool CanCheat => supercheatActivated || PlayerData.CurrentProfileName == cheatProfileName;
#endif


        public void StartCheat(string cheatName)
        {
            foreach (CheatInfo cheat in CheatSettings.CheatInfos)
            {
                if (cheat.method.Name == cheatName)
                {
                    if (cheat.autoCloseMenu)
                        Show = false;

                    Call(cheat);
                    break;
                }
            }
        }

        private static GUIStyle ButtonStyle =>
            _buttonStyle ?? (guiSkin ? (_buttonStyle = guiSkin.button) : (_buttonStyle = new GUIStyle(GUI.skin.box)
            {
                clipping = TextClipping.Overflow,
                richText = true,
                normal = {background = Texture2D.whiteTexture, textColor = Color.black},
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                active = {background = Texture2D.blackTexture, textColor = Color.cyan}
            }));


        private float beforeShowTimeScale = 1;

        public bool Show
        {
            get { return show; }
            set
            {
                show = value && CanCheat;

                if(show)
                {
                    beforeShowTimeScale = Time.timeScale;
                    Time.timeScale = 0;
                }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                else if(Time.timeScale==0)
                {
                    Time.timeScale = beforeShowTimeScale;
                }

                builder = null;
                foreach (UnityEngine.EventSystems.EventSystem eventSystem in FindObjectsOfType<UnityEngine.EventSystems.EventSystem>())
                {
                    eventSystem.enabled = !show;
                    if(!show)
                    {
                        eventSystem.UpdateModules();
                    }
                }
            }
        }

        private CheatSettings cheatSettings;
        private readonly EventSystemRef<PlayerDataEventContainer, CoreEventSystem> eventSystem = new EventSystemRef<PlayerDataEventContainer, CoreEventSystem>();


        private CheatMenuItemSelector cheatMenuItemSelector;
        void Awake()
        {
            using (var events = eventSystem.RegisterContext(this))
            {
                events?.onProfileSelected.AddListener(OnProfileSelected);
            }

            cheatMenuItemSelector = GetCheatItemSelector();
        }

        protected virtual CheatMenuItemSelector GetCheatItemSelector()
        {
            return new CheatMenuItemSelector(this);
        }

        private void OnDestroy()
        {
            eventSystem.Value.RemoveFromAllEvents(this);
        }

        private void OnProfileSelected(int arg0)
        {
            ListenInputs(CanCheat);
        }

        void OnEnable()
        {
            if(!CanCheat)
            {    
                enabled = false;
                return;
            }

            ListenInputs(CanCheat);
            cheatSettings = CheatSettings.HasAsset ? CheatSettings.Instance : new CheatSettings{
            cheatShortcuts = new CheatShortcut[0],
            cheatsToHide = new CheatMethodName[0]
            
            };
            if(cheatSettings)
            {
                cheatShortcuts = cheatSettings.cheatShortcuts;
                cheatsToHide = new List<CheatMethodName>(cheatSettings.cheatsToHide);
                menuItemSizeInInch = cheatSettings.menuItemSizeInInch;
                guiSkin = cheatSettings.guiSkin;
                CheatSettings.Setup();
                foreach (CheatShortcut shortcut in cheatShortcuts)
                {
                    if (shortcut.cheatInfo == null)
                    {
                        UnityEngine.Debug.LogWarning("Shortcut " + shortcut.name + " ("  + shortcut.key.ToString() +") refers to an unknown function");
                    }
                }
            }
            else
            {
                cheatShortcuts = null;
                cheatsToHide = null;
                guiSkin = null;
                menuItemSizeInInch = new Vector3(3,1);
            }

            ModifyScaleForCurrentResolution();
            unscaledItemSize = menuItemSizeInInch;
            UserRescaleMenu(0);
        }

        public void UserRescaleMenu(float delta)
        {
            menuScale.Value = menuScale + delta;
            menuItemSizeInInch = unscaledItemSize * menuScale;
        }

        private void ModifyScaleForCurrentResolution()
        {
            var currentReso = Screen.currentResolution;
            float maxWidth = 0;
            float maxHeight = 0;
            foreach (Resolution resolution in Screen.resolutions)
            {
                if (maxWidth < resolution.width)
                    maxWidth = resolution.width;
                if (maxHeight < resolution.height)
                    maxHeight = resolution.height;
            }

            if (currentReso.width > 0 && currentReso.height > 0)
            {
                float scaleX = maxWidth/currentReso.width;
                float scaleY = maxHeight/currentReso.height;

                if (scaleX > 0)
                    menuItemSizeInInch.x *= scaleX;
                if (scaleY > 0)
                    menuItemSizeInInch.y *= scaleY;
            }
        }


        void OnDisable()
        {
            ListenInputs(false);

            cheatShortcuts = null;
            cheatSettings = null;
        }

        void Update()
        {
            if (!IsListeningInputs)
                return;
            CheckInputs();
        }

        protected virtual void CheckInputs()
        {
            CheckOpenGesture();
            if (Input.anyKeyDown && keysActive)
            {
                CheckOpenKeys();
                CheckShortcuts();
            }

            if (Show || builder != null)
            {
                scrollDirection = (builder?.Update() ?? cheatMenuItemSelector.Update());
            }
        }

        protected virtual void CheckOpenKeys()
        {
            if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.F7))
                Show = !Show;
        }

        protected virtual void CheckOpenGesture()
        {
            Vector3 mousePosition = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                touchedDown = mousePosition.y < Screen.height * 0.25f
                           && mousePosition.x > Screen.width * 0.75f;
            }

            if (touchedDown && Input.GetMouseButtonUp(0))
            {
                if (mousePosition.y > Screen.height * 0.75f 
                 && mousePosition.x < Screen.width * 0.25f)
                {
                    Show = !Show;
                }
            }
        }

        protected virtual void CheckShortcuts()
        {
            if (builder != null) return;
            if (cheatShortcuts == null)
                return;
            foreach (var cheatShortcut in cheatShortcuts)
            {
                if (Input.GetKeyDown(cheatShortcut.key))
                {
                    if (cheatShortcut.cheatInfo != null)
                    {
                        Call(cheatShortcut.cheatInfo);
                        if (cheatShortcut.cheatInfo.autoCloseMenu)
                        {
                            Show = false;
                        }

                        if (cheatShortcut.cheatInfo.method.GetParameters().Length > 0)
                            break;
                    }
                    else
                    {
                        CheatMessages.Display("Cheat shortcut " + cheatShortcut.key.ToString().InBold() +
                                              " linked to unknown function " + cheatShortcut.name.InColor(Color.red));
                    }
                }
            }
        }

        /// <summary>
        /// Changes weather or not the CheatManager should listen to the user inputs.
        /// </summary>
        /// <param name="listen"></param>
        public void ListenInputs(bool listen)
        {
            IsListeningInputs = listen;
            enabled = listen;
        }

        private static CallBuilder lastBuilder;
        private CallBuilder builder;
        
        [CheatFunction(CheatCategory.Global, true)]
        private static void RepeatLastCheat()
        {
            if(lastBuilder==null)
            {
                CheatMessages.Display("No previous cheat");
                return;
            }
            lastBuilder.ExecuteAgain();
        }

        private static string LogsStatus()
        {
            return Debug.unityLogger.logEnabled ? "Disable logs" : "Enable Logs";
        }

        [CheatFunction(CheatCategory.Global, true, CustomMenuDisplay = nameof(LogsStatus))]
        private static void ToggleLogs()
        {
            Debug.unityLogger.logEnabled = !Debug.unityLogger.logEnabled;
            if (!Debug.unityLogger.logEnabled)
            {
                Debug.developerConsoleVisible = false;
                Debug.ClearDeveloperConsole();
            }
        }
        
        protected virtual CallBuilder GetBuilder(CheatInfo cheatInfo)
        {
            return new CallBuilder(cheatInfo, this);
        }

        private void Call(CheatInfo cheatInfo)
        {
            ParameterInfo[] parameterInfos = cheatInfo.method.GetParameters();
            if (parameterInfos.Length == 0)
            {
                cheatInfo.Execute(new object[] { });
                return;
            }
            builder = GetBuilder(cheatInfo);
        }

        protected virtual Matrix4x4 GetGUIMatrix()
        {
            var matrix = GUI.matrix;
            matrix.m03 = cheatSettings.menuItemPosInInch.x;
            matrix.m13 = cheatSettings.menuItemPosInInch.y;
            return matrix;
        }
//#if CHEATS_ENABLED
        void OnGUI()
        {
            if(!IsListeningInputs)
                return;

            CheatMessages.OnGUI();
            GUI.matrix = GetGUIMatrix();

            if (builder != null)
            {
                BeginScrollView();
                switch(builder.GatherParametersForCall(menuItemSizeInInch))
                {
                    case CallBuilder.State.Cancel:
                        builder = null;
                        break;
                    case CallBuilder.State.Ready:
                        lastBuilder = builder;
                        builder = null;
                        break;
                }

                GUILayout.EndScrollView();

                return;
            }

            if (!Show)
            {
                return;
            }
            GUISkin previousSkin = GUI.skin;
            if(guiSkin)
                GUI.skin = guiSkin;
            BeginScrollView();
            cheatMenuItemSelector.OnGUI();
            GUILayout.EndScrollView();
            GUI.skin = previousSkin;
        }
//#endif
        private void BeginScrollView()
        {
            var screenDpi = Screen.dpi;
            if (screenDpi == 0)
                screenDpi = 200;
            
            float barSize = screenDpi * menuItemSizeInInch.y;
            GUISkin skin = GUI.skin;
            GUIStyle verticalScrollbar = skin.verticalScrollbar;
            GUIStyle verticalScrollbarThumb = skin.verticalScrollbarThumb;
            GUIStyle horizontalScrollbar = skin.horizontalScrollbar;
            GUIStyle horizontalScrollbarThumb = skin.horizontalScrollbarThumb;

            verticalScrollbar.fixedWidth =
                verticalScrollbarThumb.fixedWidth =
                    horizontalScrollbarThumb.fixedHeight =
                        horizontalScrollbar.fixedHeight = barSize;
            if (skin != guiSkin)
            {
                verticalScrollbarThumb.normal.background =
                horizontalScrollbarThumb.normal.background = Texture2D.whiteTexture;

                verticalScrollbar.normal.background =
                horizontalScrollbar.normal.background = Texture2D.blackTexture;
            }
            scrollPos = GUILayout.BeginScrollView(scrollPos, 
                GUILayout.Width(Screen.width - cheatSettings.menuItemPosInInch.x), 
                GUILayout.Height(Screen.height - cheatSettings.menuItemPosInInch.y)) + scrollDirection; 
        }

        protected virtual bool ShouldShowCategory(CheatCategory category)
        {
            return true;
        }

        protected virtual bool ShouldShowCheat(string methodName)
        {
            return cheatsToHide?.Find(x=> x.Name == methodName) == null;
        }

        static readonly List<GUIContent> guiContents = Enum.GetNames(typeof(CheatCategory)).Select(s => new GUIContent(s)).ToList();
        static readonly Array categoryValues = Enum.GetValues(typeof(CheatCategory));
        static public bool keysActive = true;
        private Vector2 scrollDirection;

        public static Rect DrawRect(GUIContent guiContent, Vector2 size)
        {
            if (!myBlackTexture)
            {
                myBlackTexture = new Texture2D(1, 1);
                myBlackTexture.SetPixel(0, 0, Color.black);
                myBlackTexture.Apply();
            }

            float height = Screen.dpi * size.y;
            int buttonStyleFontSize = (int) (height * 0.66f);
            ButtonStyle.fontSize = buttonStyleFontSize;
#if !UNITY_EDITOR
            GUI.skin.textField.fontSize = buttonStyleFontSize;
#endif
            ButtonStyle.fixedWidth = 0f;

            float textSize = ButtonStyle.CalcSize(guiContent).x;

            ButtonStyle.fixedWidth = size.x * Screen.dpi;
            ButtonStyle.fixedHeight = height;

            if (textSize > ButtonStyle.fixedWidth)
            {
                ButtonStyle.fontSize = (int)((ButtonStyle.fixedWidth * ButtonStyle.fontSize) / textSize);
            }

            var rect = GUILayoutUtility.GetRect(guiContent, ButtonStyle
                , GUILayout.MaxWidth(size.x * Screen.dpi));

            GUI.DrawTexture(rect, myBlackTexture); // Texture2D.blackTexture is c**p
            var off = new Vector2(1, 1);
            rect.min += off;
            rect.max -= off;
            return rect;
        }

        public static bool Button(GUIContent guiContent, Vector2 size)
        {
            var rect = DrawRect(guiContent, size);
            return GUI.Button(rect, guiContent, ButtonStyle);
        }

        public static bool Button(string content, Vector2 size)
        {
            GUIContent guiContent = new GUIContent(content);
            var rect = DrawRect(guiContent, size);
            return GUI.Button(rect, guiContent, ButtonStyle);
        }


        public static void Label(GUIContent guiContent, Vector2 size)
        {
            var rect = DrawRect(guiContent, size);
            GUI.Label(rect, guiContent, ButtonStyle);
        }

#if !CHEATS_ENABLED
        [CheatFunction(CheatCategory.Global, true)]
        private static void ActiveCheatsForOtherProfiles()
        {
            supercheatActivated = true;
        }
#endif

        /*
        [CheatFunction(CheatCategory.Global, true)]
        private static void TestCheat(int anInteger, float aFloat, string aString)
        {
            UnDebug.Log("intValue " + anInteger +
                      " floatValue " + aFloat +
                      " stringValue " + aString);
        }*/
    }
}





