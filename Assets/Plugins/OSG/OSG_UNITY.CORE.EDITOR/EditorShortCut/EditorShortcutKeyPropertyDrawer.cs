using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    [CustomPropertyDrawer(typeof(EditorShortcutKeys))]
    public class EditorShortcutKeyPropertyDrawer : SelectFieldDrawerBase<EditorShortcutKeys>
    {
     //   public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
     //   {
     //       float propCount = 0;
     //       SelectedField.Enumerate(property, typeof(EditorShortcutKeys), s => ++propCount);
     //       return EditorGUIUtility.singleLineHeight * propCount;
     //   }
     //
        string name;

        private void sceneObjectTypeGUI(Rect position, SerializedProperty serializedProperty)
        {
            TypeMenu(position, serializedProperty, EditorShortcutManager.GetSceneObjectsMenu);
        }

        private delegate GenericMenu MenuForType<Type>(Action<Type> doAction);

        private void TypeMenu(Rect position, SerializedProperty serializedProperty, MenuForType<Type> menuAction)
        {
            if (GUI.Button(position, name))
            {
                GenericMenu menu = menuAction(type =>
                    {
                        serializedProperty.stringValue = type.AssemblyQualifiedName;
                        serializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                );
                menu.DropDown(position);
            }
        }

        private void scriptableTypeGUI(Rect position, SerializedProperty serializedProperty)
        {
            TypeMenu(position, serializedProperty, EditorShortcutManager.GetScriptablesMenu);
        }
        
        private void codeGUI(Rect position, SerializedProperty serializedProperty)
        {
            if (GUI.Button(position, serializedProperty.enumNames[serializedProperty.enumValueIndex]))
            {
                Rect p = position;
                p.position = GUIUtility.GUIToScreenPoint(p.position);
                WaitForKey.Open(p, serializedProperty, "Select key to " + name.InColor(Color.yellow));
            }
        }

        private void methodGUI(Rect position, SerializedProperty methodProperty)
        {
            //object targetObject;
            //FieldInfo methodFieldInfo = methodProperty.GetRealObjectFieldInfo(out targetObject);

            {
                //if (shouldClearMethod)
                //{
                //    UnityEngine.Debug.Log("Clear the method we got a new one");
                //    SetMethodProperty(methodProperty, methodFieldInfo, targetObject,
                //        new SerializableMethodInfo());
                //}

              //  SerializableMethodInfo mi = targetObject != null
              //      ? methodFieldInfo.GetValue(targetObject) as SerializableMethodInfo
              //      : null;

                SerializableMethodInfo mi;

                methodProperty.GetObjectValue(out mi);


                GUIContent dropButtonContent = mi == null
                    ? new GUIContent("Select one:")
                    : mi.Info.GUIContent(false);

                if (EditorGUI.DropdownButton(position, dropButtonContent, FocusType.Keyboard,
                    SerializedPropertyExtensions.DropButtonStyle))
                {
                    GenericMenu menu = new GenericMenu();

                    foreach (MethodInfo info in EditorShortcutManager.Methods)
                    {
                        var info1 = info;
                        menu.AddItem(new GUIContent(EditorShortcutManager.GetContent(info)), false, () =>
                        {
                            mi = new SerializableMethodInfo
                            {
                                Info = info1,
                                bindingFlags = (int)EditorShortcutManager.MethodsBindingFlags
                            };
                            SetMethodProperty(methodProperty, mi);
                        });
                    }
                    menu.DropDown(position);
                }
            }
        }

        private static void SetMethodProperty(SerializedProperty methodProperty,
            SerializableMethodInfo serializableMethodInfo)
        {
            Undo.RecordObject(methodProperty.serializedObject.targetObject, "Set Method");
            methodProperty.SetObjectValue(serializableMethodInfo);
            EditorUtility.SetDirty(methodProperty.serializedObject.targetObject);
            methodProperty.serializedObject.ApplyModifiedProperties();
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //object obj;
            //property.GetRealObjectFieldInfo(out obj);

            EditorShortcutKeys k ;//= (EditorShortcutKeys)obj;

            property.GetObjectValue(out k);
            name = k.ToString();
            base.OnGUI(position, property, label);
        }
      
    }
}