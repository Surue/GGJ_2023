// Old Skull Games
// Bernard Barthelemy
// Wednesday, January 16, 2019

using System;
using OSG.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OSG
{
    [CustomPropertyDrawer(typeof(SerializedType), true)]
    class SerializedTypePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedType field;
            property.GetObjectValue(out field);
            Type currentType = field;
            var splits = position.HorizontalSplit(0.425f, 0.575f);
            GUI.Label(splits[0], label);
            Color color = GUI.color;
            GUI.color = currentType.IsAbstract ? Color.red : Color.white;

            if (EditorGUI.DropdownButton(splits[1], new GUIContent(currentType.Name), FocusType.Keyboard))
            {
                Type baseType = field.baseType;
                GenericMenu menu = new GenericMenu();
                AssemblyScanner.Register(type =>
                {
                    if (type.IsAbstract)
                    {
                        return;
                    }

                    if (type.DerivesFrom(baseType))
                    {
                        menu.AddItem(new GUIContent(type.Name), type == currentType,
                            () =>
                            {
                                field.SetEffectiveType(type);
                                UnityEngine.Object target = property.serializedObject.targetObject;
                                EditorUtility.SetDirty(target);
                                Component c = target as Component;
                                if (c && c.gameObject.scene.rootCount > 0)
                                {
                                    EditorSceneManager.MarkSceneDirty(c.gameObject.scene);
                                }
                            }
                        );
                    }
                });
                AssemblyScanner.Scan(() => menu.ShowAsContext());
            }

            GUI.color = color;
        }
    }
}
