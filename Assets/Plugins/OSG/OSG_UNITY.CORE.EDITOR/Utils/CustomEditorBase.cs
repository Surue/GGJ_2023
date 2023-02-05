
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OSG
{
    [InitializeOnLoad]
    static class MenuInitializer
    {
        static MenuInitializer()
        {
            EditorListener.OnNextUpdateDo(CustomEditorBase.InitMenu);
        }
    }


    [CustomEditor(typeof(Object), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class CustomEditorBase : Editor
    {
        const string defaultKey = "OSG_USE_DEFAULT_INSPECTOR";
        const string skullSpriteName = "Skull.png";
        private const string MENU_NAME = "OSG/Use OSG Inspector";

        public static string autofocus;

        private static event Action OnOptionChange;

        private static bool useDefault
        {
            get
            {
                return EditorPrefs.GetBool(defaultKey, false);
            }
            set
            {
                EditorPrefs.SetBool(defaultKey, value);
            }
        }

        private static Sprite _skullSprite;
        protected static Sprite skullSprite
        {
            get
            {
                if (!_skullSprite)
                    _skullSprite = FindGizmoSpriteByName(skullSpriteName);
                return _skullSprite;
            }
        }

        private EditorFieldSelector selector;
        
        private Dictionary<string, ReorderableListProperty> reorderableLists;

        private GUIContent scriptName;
        private GUIStyle nameStyle;

        protected bool isOsgMono;
        
        public static void InitMenu()
        {
            EditorListener.OnNextUpdateDo(()=> Menu.SetChecked(MENU_NAME, !useDefault));
        }

        [MenuItem(MENU_NAME)]
        public static void ToggleInspector()
        {
            useDefault = !useDefault;
            InitMenu();
            if (OnOptionChange != null)
            {
                OnOptionChange.Invoke();
            }
        }

        #region Initialization

        protected virtual void OnEnable()
        {
            try{
                if(!serializedObject.targetObject)
                    return;
            }
            catch(Exception )
            {
                return;
            }
            

            CustomEditorBase.OnOptionChange += Repaint;
            InitMenu();

            reorderableLists = new Dictionary<string, ReorderableListProperty>(10);
            isOsgMono = serializedObject.targetObject.GetType().DerivesFrom(typeof(OSGMono));
            if(isOsgMono)
            {
                scriptName = new GUIContent("OSGMono");

                ApplyEditorMode();
            }
            else
            {
                scriptName = new GUIContent("Script");
            }

            //selector = new EditorFieldSelector(this);
        }
        

        protected virtual void ApplyEditorMode()
        {
            
        }

        protected static void ShowComponentInInspector(Component component, bool show)
        {
            component.hideFlags = show ? HideFlags.None : HideFlags.HideInInspector;
        }

        protected virtual void OnDisable()
        {
            CustomEditorBase.OnOptionChange -= Repaint;
        }

        ~CustomEditorBase()
        {
            if (reorderableLists != null)
            {
                reorderableLists.Clear();
            }
            reorderableLists = null;
        }

        #endregion

        public override void OnInspectorGUI()
        {
            //Event current = Event.current;
            //if (current != null && current.type == EventType.MouseDown && current.button==1)
            //{
            //    GenericMenu menu = new GenericMenu();
            //    menu.AddItem(new GUIContent(useDefault ? "Use OSG Inspector" : "Use default Inspector"), false, () => useDefault=!useDefault);
            //    menu.ShowAsContext();
            //}

            if (useDefault)
            {
                base.OnInspectorGUI();
                return;
            }

            if (!serializedObject.targetObject)
            {
                EditorGUILayout.HelpBox("Missing Script", MessageType.Error);
                return;
            }
            
            serializedObject.Update();
            //foreach (var info in methodToCallForProperty)
            //{
                //info.Value.Invoke(info.Key);
            //}
            selector = new EditorFieldSelector(this); // DON'T CACHE it !!!!
            if(selector!=null)
                selector.OnGUI();

            Color cachedGuiColor = GUI.color;
            
            var property = serializedObject.GetIterator();
            var next = property.NextVisible(true);
            if (next)
                do
                {
                    GUI.color = cachedGuiColor;
                    HandleProperty(property);
                } while (property.NextVisible(false));

            if (!string.IsNullOrEmpty(autofocus))
            {
                GUI.FocusControl(autofocus);
                autofocus = null;
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected bool HandleProperty(SerializedProperty property)
        {
            //Debug.LogFormat("name: {0}, displayName: {1}, type: {2}, propertyType: {3}, path: {4}", property.name, property.displayName, property.type, property.propertyType, property.propertyPath);
            bool isdefaultScriptProperty = property.name.Equals("m_Script") && property.type.Equals("PPtr<MonoScript>") && property.propertyType == SerializedPropertyType.ObjectReference && property.propertyPath.Equals("m_Script");
            bool cachedGUIEnabled = GUI.enabled;
            if (isdefaultScriptProperty)
                GUI.enabled = false;
            //var attr = this.GetPropertyAttributes(property);
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
                HandleArray(property);
            else if (isdefaultScriptProperty)
            {
                if (isOsgMono)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = true;
                    float size = 2 * EditorGUIUtility.singleLineHeight;
                    GUILayout.Box(staticPreviewTexture, GUILayout.Width(size), GUILayout.Height(size));
                    GUI.enabled = false;
                }

                HandleNonArray(property);

                GUI.enabled = cachedGUIEnabled;
                if (isOsgMono)
                {
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                HandleNonArray(property);
            }

            if (isdefaultScriptProperty)
                GUI.enabled = cachedGUIEnabled;

            return isdefaultScriptProperty;
        }

        /// <summary>
        /// handle special attribute for NonArray property.
        /// </summary>
        /// <param name="property"></param>
        protected void HandleNonArray(SerializedProperty property)
        {
            FieldInfo info = property.GetFieldInfo();
            ConditionalHideAttribute conditionnalHideAttribute = null;
            if (info != null)
            {
                var query = (from attribute in info.GetCustomAttributes(false) where (attribute is ConditionalHideAttribute) select attribute);
                if (query.Count() > 0)
                    conditionnalHideAttribute = query.First() as ConditionalHideAttribute;
            }


            ConditionalHideFunctionAttribute conditionnalHideFunctionAttribute = null;
            if (info != null)
            {
                var query = (from attribute in info.GetCustomAttributes(false) where (attribute is ConditionalHideFunctionAttribute) select attribute);
                if (query.Count() > 0)
                    conditionnalHideFunctionAttribute = query.First() as ConditionalHideFunctionAttribute;
            }

            if ((conditionnalHideFunctionAttribute == null && conditionnalHideAttribute == null)
                || (conditionnalHideFunctionAttribute != null && conditionnalHideFunctionAttribute.GetConditionalHideAttributeResult(info.DeclaringType, () => { return property.GetOwner(); }))
                || (conditionnalHideAttribute != null && conditionnalHideAttribute.PropertyIsVisible(property.GetOwner())))
            {
                EditorGUILayout.PropertyField(property, property.isExpanded);
            }

        }

        protected void HandleArray(SerializedProperty property)
        {
            ReorderableListProperty listData = GetReorderableList(property);
            //listData.IsExpanded.target = property.isExpanded;
            //if ((!listData.IsExpanded.value && !listData.IsExpanded.isAnimating) || (!listData.IsExpanded.value && listData.IsExpanded.isAnimating))

            //object dontCare;
            //FieldInfo infow = property.GetRealObjectFieldInfo(out dontCare);
            FieldInfo info = property.GetFieldInfo();
            if (info != null)
            {
                listData.tooltip = null;

                foreach (Attribute customAttribute in info.GetCustomAttributes(false))
                {

                    var res = ManageAttribute(property, customAttribute);
                    switch (res)
                    {
                        case AttributeAction.None:
                            break;
                        case AttributeAction.DontShow:
                            return;
                        case AttributeAction.Deactivate:
                            GUI.enabled = false;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    if (!string.IsNullOrEmpty(tooltip))
                        listData.tooltip = tooltip;
                }
            }


//            if (listData.List.count == 0)
//            {
//                
//                EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(0));
//            }
            
            if(!property.isExpanded)
            {
                //EditorGUILayout.BeginHorizontal();
                //bool isExpanded = EditorGUILayout.ToggleLeft(string.Format("{0}[]",property.displayName), 
                //    property.isExpanded, 
                //    EditorStyles.boldLabel);

                //property.isExpanded = isExpanded;
                //EditorGUILayout.LabelField(string.Format("size: {0}", property.arraySize));
                //EditorGUILayout.EndHorizontal();
                if (listData.List.count == 1)
                {
                    GUILayout.BeginHorizontal();
                    property.isExpanded = EditorGUILayout.Foldout(false, property.displayName);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(0), GUIContent.none);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    listData.List.DrawOnlyHeader();
                }
            }
            else
            {
                
                listData.List.DoLayoutList();
            }
        }

        private static GUIStyle headerStyle;
        public static GUIStyle HeaderStyle
        {
            get
            {
                if (headerStyle == null)
                {
                    headerStyle = new GUIStyle(EditorStyles.largeLabel) {fontStyle = FontStyle.Bold};
                }
                return headerStyle;
            }
        }

        private string tooltip;

        protected enum AttributeAction
        {
            None,
            DontShow,
            Deactivate
        }

        protected virtual AttributeAction ManageAttribute(SerializedProperty property, Attribute customAttribute)
        {
            HeaderAttribute headerAttribute = customAttribute as HeaderAttribute;
            if (headerAttribute != null)
            {
                GUILayout.Label(headerAttribute.header, HeaderStyle);
                return AttributeAction.None;
            }

            if (customAttribute is LocalizedTextAttribute)
                return AttributeAction.None;
            
            if(customAttribute is MinMaxRangeAttribute)
                return AttributeAction.None;

            SpaceAttribute spaceAttribute = customAttribute as SpaceAttribute;
            if (spaceAttribute != null)
            {
                GUILayout.Space(spaceAttribute.height);
                return AttributeAction.None;
            }

            TooltipAttribute tooltipAttribute = customAttribute as TooltipAttribute;
            if (tooltipAttribute != null)
            {
                tooltip = tooltipAttribute.tooltip;
                return AttributeAction.None;
            }
            else
                tooltip = null;

            ConditionalHideAttribute condHide = customAttribute as ConditionalHideAttribute;
            if(condHide != null)
            {
                object owner = property.GetOwner();
                return owner == null || condHide.PropertyIsVisible(owner)
                    ? AttributeAction.None
                    : AttributeAction.DontShow;
            }

            ConditionalHideFunctionAttribute condHideFun = customAttribute as ConditionalHideFunctionAttribute;
            if(condHideFun != null)
            {
                object owner = property.GetOwner();
                return owner == null || condHideFun.GetConditionalHideAttributeResult(owner.GetType(), () => owner)
                    ? AttributeAction.None
                    : (condHideFun.HideInInspector ? AttributeAction.DontShow : AttributeAction.Deactivate);
            }

            PropertyAttribute propertyAttribute = customAttribute as PropertyAttribute;
            if (propertyAttribute != null)
            {
                EditorGUILayout.HelpBox(customAttribute.GetType().Name + " on arrays or List is not managed yet by CustomEditorBase.ManageAttribute. Please add it", MessageType.Error);
            }

            return AttributeAction.None;
        }

        protected object[] GetPropertyAttributes(SerializedProperty property)
        {
            return GetPropertyAttributes<PropertyAttribute>(property);
        }

        protected object[] GetPropertyAttributes<T>(SerializedProperty property) where T : Attribute
        {
            BindingFlags bindingFlags = BindingFlags.GetField
                | BindingFlags.GetProperty
                | BindingFlags.IgnoreCase
                | BindingFlags.Instance
                | BindingFlags.NonPublic
                | BindingFlags.Public;
            if (property.serializedObject.targetObject == null)
                return null;
            var targetType = property.serializedObject.targetObject.GetType();
            var field = targetType.GetField(property.name, bindingFlags);
            return field != null ? field.GetCustomAttributes(typeof(T), true) : null;
        }

        private ReorderableListProperty GetReorderableList(SerializedProperty property)
        {
            ReorderableListProperty ret = null;
            if (reorderableLists.TryGetValue(property.name, out ret))
            {
                ret.Property = property;
                return ret;
            }
            ret = new ReorderableListProperty(property);
            reorderableLists.Add(property.name, ret);
            return ret;
        }

        protected virtual void DisplayHelpBoxMessage(string message, MessageType messageType = MessageType.Info)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox(message, messageType);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        #region Static Preview

        private Texture2D _staticPreviewTexture;
        protected virtual Texture2D staticPreviewTexture
        {
            get
            {
                if (_staticPreviewTexture)
                    return _staticPreviewTexture;
                Sprite sprite = FindGizmoSpriteOfType(target.GetType());
                _staticPreviewTexture = sprite ? sprite.texture : Texture2D.whiteTexture;
                return _staticPreviewTexture;
            }
        }

        private static Sprite FindGizmoSpriteByName(string typeName)
        {
            Sprite gizmoSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Gizmos/{typeName}") ??
                                 AssetDatabase.LoadAssetAtPath<Sprite>(
                                     $"Packages/com.osg.unitycore.editor/DefaultResources/Gizmos/{typeName}");
            return gizmoSprite;
        }

        protected virtual Sprite FindGizmoSpriteOfType(Type type)
        {
            Sprite gizmoSprite = FindGizmoSpriteByName($"{type.Name} Icon.png");

            if (gizmoSprite)
                return gizmoSprite;

            Type currentType = type.BaseType;
            int i = 0;
            while (currentType != typeof(Object) && (currentType != null))
            {
                gizmoSprite = FindGizmoSpriteByName($"{currentType.Name} Icon.png");

                if (gizmoSprite)
                    return gizmoSprite;

                currentType = currentType.BaseType;
                
                if (i++ > 20)
                {
                    UnityEngine.Debug.LogError("Unable to automatically stop searching for an Icon. This is serious, come check what happened.");
                    break;
                }
            }

            return skullSprite;
        }

        protected Texture2D cachedPreview2D;
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (!cachedPreview2D || (cachedPreview2D.width != staticPreviewTexture.width || cachedPreview2D.height != staticPreviewTexture.height))
            {
                cachedPreview2D = new Texture2D(staticPreviewTexture.width, staticPreviewTexture.height);

                try
                {
                    cachedPreview2D.SetPixels(staticPreviewTexture.GetPixels());
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning(e.Message);
                    UnityEngine.Debug.Log("Using Skull Sprite instead.");

                    Texture2D skullTex = skullSprite.texture;
                    cachedPreview2D = new Texture2D(skullTex.width, skullTex.height);
                    cachedPreview2D.SetPixels(skullTex.GetPixels());
                }

                cachedPreview2D.Apply();
            }

            return cachedPreview2D;
        }

        #endregion

        #region Inner-class ReorderableListProperty
        protected class ReorderableListProperty
        {
            //public AnimBool IsExpanded { get; private set; }
           // public bool isExpanded;

            /// <summary>
            /// ref http://va.lent.in/unity-make-your-lists-functional-with-reorderablelist/
            /// </summary>
            public ReorderableList List { get; private set; }

            public string tooltip = null;

            private SerializedProperty _property;
            public SerializedProperty Property
            {
                get { return _property; }
                set
                {
                    _property = value;
                    List.serializedProperty = _property;
                }
            }

            public ReorderableListProperty(SerializedProperty property)
            {
                //IsExpanded = new AnimBool(property.isExpanded);
                //IsExpanded.speed = 1f;
                _property = property;
                CreateList();
            }

            ~ReorderableListProperty()
            {
                _property = null;
                List = null;
            }

            private void CreateList()
            {
                bool dragable = true, header = true, add = true, remove = true;
                List = new ReorderableList(Property.serializedObject, Property, dragable, header, add, remove)
                {
                    onAddCallback = OnAddElement,
                    drawHeaderCallback = OnListDrawHeaderCallback,
                    onCanRemoveCallback = list => List.count > 0,
                    drawElementCallback = drawElement,
                    elementHeightCallback =
                        idx => Mathf.Max(EditorGUIUtility.singleLineHeight,
                                   EditorGUI.GetPropertyHeight(_property.GetArrayElementAtIndex(idx), GUIContent.none, true)) + 4.0f
                };
            }

            private void OnAddElement(ReorderableList list)
            {
                //UnityEngine.Debug.Log("On Add Element " + list.serializedProperty.displayName);
                ++list.serializedProperty.arraySize;
                list.index = list.serializedProperty.arraySize - 1;
                list.serializedProperty.serializedObject.ApplyModifiedProperties();

                SerializedProperty arrayElementAtIndex = list.serializedProperty.GetArrayElementAtIndex(list.index);
                arrayElementAtIndex.SetNewValue();
            }

            private void OnListDrawHeaderCallback(Rect rect)
            {
                Rect r = rect;
                r.width = 16;
                _property.isExpanded = EditorGUI.Foldout(r, _property.isExpanded, GUIContent.none);

                if (GUI.Button(rect, new GUIContent(_property.displayName, tooltip), "Label"))
                {
                    _property.isExpanded = !_property.isExpanded;
                }

            }

            GUIStyle indexStyle = "MiniLabel";

            public void drawElement(Rect rect, int index, bool active, bool focused)
            {
                //if (this._property.GetArrayElementAtIndex(index).propertyType == SerializedPropertyType.Generic)
                //{
                //    EditorGUI.LabelField(rect, this._property.GetArrayElementAtIndex(index).displayName);
                //}
                //rect.height = 16;


                string name = _property.serializedObject.targetObject.name+"."+_property.name+index;
                
                GUI.SetNextControlName(name);
                GUI.Label(rect, "");

                int nbChars = _property.arraySize > 1 ? Mathf.FloorToInt(Mathf.Log10(_property.arraySize-1))+1 : 1;
                Rect indexRect = rect;
                var size = indexStyle.CalcSize(new GUIContent("8"));
                indexRect.width = size.x * nbChars - 1;
                indexRect.x -= 6;
                indexRect.y += 1;
                GUI.Label(indexRect, index.ToString(), indexStyle);

                rect.xMin = indexRect.xMax;
                rect.height = EditorGUI.GetPropertyHeight(_property.GetArrayElementAtIndex(index), GUIContent.none, true);
                rect.y += 1;
                EditorGUI.PropertyField(rect, _property.GetArrayElementAtIndex(index), GUIContent.none, true);
                List.elementHeight = rect.height + 4.0f;
            }
        }


        protected const string resourcesFolderName = "/Resources/";
        protected string AbsoluteToRelativeResourcesPath(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
                return absolutePath;

            int resourcesIndex = absolutePath.IndexOf(resourcesFolderName);

            if (resourcesIndex < 0)
                return absolutePath;

            string relativePath = absolutePath.Substring(resourcesIndex + resourcesFolderName.Length);
            int dotIndex = relativePath.LastIndexOf('.');
            int relativePathLength = relativePath.Length;

            return relativePath.Substring(0, relativePathLength - (relativePathLength - dotIndex));
        }


        #endregion
    }
}
