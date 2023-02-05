using System;
using System.Collections.Generic;
using System.Reflection;
using OSG;
using UnityEditor;
using UnityEngine;

public abstract class BaseFieldSelectorData
{
    protected abstract class BaseCallData
    {
        protected SerializedProperty property;

        protected BaseCallData(SerializedProperty p)
        {
            property = p;
        }
    }    

    
    protected abstract Type baseType{get;}
    
    protected Delegate GetMethod(object owner, Type callType, string methodName)
    {
        Type type = owner.GetType();
        // FlattenHierachy should have allowed us to avoid trying manually every
        // base type, but this bovine droppin does nothing for us... so... loop
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                                          BindingFlags.FlattenHierarchy; // <= this came from a northbound bull's southern side.
        while (type != null)
        {
            MethodInfo info = type.GetMethod(methodName, bindingFlags);
            if (info != null)
            {
                try{
                    return Delegate.CreateDelegate(callType, owner, info);
                }
                catch(Exception e)
                {
                    Debug.LogWarning(
                        "Looking for " + callType.Name + " " + methodName + " " + 
                        e.Message.InColor(Color.red));
                    string text = "Found: " + methodName + "(";
                    bool first = true;
                    foreach (var parameterInfo in info.GetParameters())
                    {
                        if (!first)
                            text += ",";
                        first=false;
                        text += parameterInfo.ParameterType.Name + " " + parameterInfo.Name;
                    }
                    text += ")";
                    Debug.Log(text);
                }
            }
            if (type == baseType)
            {
                break;
            }
            type = type.BaseType;
        }
        return null;
    }

    protected void DefaultEditProperty(SerializedProperty property)
    {
        if(property.isArray)
        {
            EditorGUILayout.HelpBox(property.name + " is an array, unity doesn't display those correctly when they are tagged [HideInInspector] please create a method "
                                    + property.name+"GUI(SerializedProperty property) in a custom editor for the type it belongs to", MessageType.Error);
        }
        else
            EditorGUILayout.PropertyField(property);
    }
    
}

public class EditorFieldSelector : BaseFieldSelectorData
{
    private delegate void EditPropertyCallback(SerializedProperty property);
    
    class CallData : BaseCallData
    {
        private EditPropertyCallback callback;
        public void Execute()
        {
            callback(property);
        }
        public CallData(EditPropertyCallback c, SerializedProperty p) : base(p)
        {
            callback=c;
        }
    }

    private readonly List<CallData> methodToCallForProperty;
    public EditorFieldSelector(CustomEditorBase editor)
    {
        methodToCallForProperty = new List<CallData>();
        
        SelectedField.Enumerate(editor.serializedObject, property =>
        {
            string methodName = property.name + "GUI";
            var methodToCall = (EditPropertyCallback)GetMethod(editor, typeof(EditPropertyCallback), methodName ) ?? DefaultEditProperty;
            var n = new CallData(methodToCall, property);
            methodToCallForProperty.Add(n);
        });        
    }

    protected override Type baseType
    {
        get { return typeof(CustomEditorBase); }
    }

    public void OnGUI()
    {
        foreach (var callback in methodToCallForProperty)
        {
            callback.Execute();
        }
    }
    
}

public class DrawerFieldSelector<T> : BaseFieldSelectorData 
{
    private delegate void EditPropertyCallback(Rect rect, SerializedProperty property);
    private delegate float GetPropertyHeightCallback(SerializedProperty property);
    private readonly List<CallData> methodToCallForProperty;

    class CallData : BaseCallData
    {
        EditPropertyCallback callback;
        GetPropertyHeightCallback heightCallback;

        public void Execute(ref Rect rect)
        {
            rect.height = heightCallback(property);
            callback(rect,property);
            rect.y += rect.height;
        }
        
        public CallData(EditPropertyCallback c, GetPropertyHeightCallback h, SerializedProperty p ) : base(p)
        {
            callback = c;
            heightCallback = h;
        }
    }

    private void DefaultEditProperty(Rect r, SerializedProperty property)
    {
        EditorGUI.PropertyField(r, property, property.isArray);
    }
    private float DefaultGetSize(SerializedProperty property)
    {
        return EditorGUIUtility.singleLineHeight;
    }
    public DrawerFieldSelector(SelectFieldDrawerBase<T> drawer, SerializedProperty serializedProperty)
    {
        methodToCallForProperty = new List<CallData>();
        
        SelectedField.Enumerate(serializedProperty, typeof(T), property =>
        {
            string methodName = property.name + "GUI";
            var methodToCall = (EditPropertyCallback) GetMethod(drawer,typeof(EditPropertyCallback), methodName) ?? DefaultEditProperty;
            var heightCall = (GetPropertyHeightCallback) GetMethod(drawer, typeof(GetPropertyHeightCallback), methodName + "Size")?? DefaultGetSize;
            var n = new CallData(methodToCall, heightCall, property);
            methodToCallForProperty.Add(n);
        });        
    }


    protected override Type baseType
    {
        get { return typeof(DrawerFieldSelector<>); }
    }
    
    public void OnGUI(Rect r)
    {
        foreach (var data in methodToCallForProperty)
        {
            data.Execute(ref r);
        }
    }
    
}