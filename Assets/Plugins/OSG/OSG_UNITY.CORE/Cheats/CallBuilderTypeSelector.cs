// Old Skull Games
// Bernard Barthelemy
// Thursday, March 5, 2020


using System;
using System.Collections.Generic;
using System.Linq;
using OSG.Core;
using UnityEngine;
using UnityEngine.Assertions;

namespace OSG
{
    public partial class CallBuilder
    {
        public class ObjectMenuButtonItem<T> : BaseCheatManager.ButtonMenuItem
        {
            private readonly string text;
            public readonly T obj;

            private CallBuilderMenuItemSelector callBuilderSelector;
            public ObjectMenuButtonItem(string text, T obj, CallBuilderMenuItemSelector selector) : base(selector)
            {
                this.text = text;
                this.obj = obj;
                OnClick = Clicked;
                content = Content;
                callBuilderSelector = selector;
            }

            private void Clicked()
            {
                Debug.Log($"{text} clicked");
                callBuilderSelector.builder.AddParameter(obj);
            }

            private GUIContent Content()
            {
                return new GUIContent(text);
            }
        }


        public class MenuItemType : BaseCheatManager.ButtonMenuItem
        {
            private GUIContent nameContent;
            public MenuItemType(Type type, CallBuilderTypeSelector selector, MenuItemTypeList parent) : base(selector)
            {
                OnFocus = () => selector.ChangeSelectionTo(parent);
                OnClick = () => selector.AddParameter(type);
                leftItem = parent;
                nameContent = new GUIContent(type.Name);
                content = () => nameContent;
            }
        }

        public class MenuItemTypeList : BaseCheatManager.ButtonMenuItem
        {
            private readonly Type firstType;
            public readonly List<MenuItemType> subMenuItems;
            public static MenuItemTypeList currentList;

            private readonly GUIContent cacheContent;

            public MenuItemTypeList(CallBuilderTypeSelector selector, List<Type> types, ref int firstIndex) : base(selector)
            {
                char firstChar = types[firstIndex].Name[0];
                int lastIndex;
                firstType = types[firstIndex];
                for(lastIndex = firstIndex; lastIndex < types.Count;++lastIndex)
                {
                    if(types[lastIndex].Name[0] != firstChar)
                    {
                        break;
                    }
                }

                int typeCount = lastIndex - firstIndex;
                if(typeCount>1)
                {
                    cacheContent = new GUIContent($"{firstChar}...");
                    
                    subMenuItems = new List<MenuItemType>(typeCount);
                    for(int i = 0; i < typeCount;++i)
                    {
                        var button = new MenuItemType(types[i+firstIndex], selector, this);
                        subMenuItems.Add(button);
                    }

                    OnClick = () =>
                    {
                        selector.ChangeSelectionTo(this);
                        currentList = this;
                        selector.ChangeFocusTo(subMenuItems[0]);
                    };

                    OnFocus = () =>
                    {
                        currentList = this;
                        selector.ChangeSelectionTo(null);
                    };

                    MakeListLoop(subMenuItems);

                    rightItem = subMenuItems[0];
                }
                else
                {
                    OnClick = () => selector.AddParameter(firstType);
                    cacheContent = new GUIContent($"{firstType.Name}");
                }

                content = () => cacheContent;
                firstIndex = lastIndex;
            }

            public bool HasSubItems => subMenuItems?.Count > 0;
        }


        public class CallBuilderTypeSelector : CallBuilderMenuItemSelector
        {
            private readonly Type wantedBaseType;
            private readonly GUIContent label;

            public CallBuilderTypeSelector(CallBuilder builder, string name, Type wantedBaseType) : base(builder, name)
            {
                this.wantedBaseType = wantedBaseType;
                mainMenuItems = new List<BaseCheatManager.BaseMenuItem>();
                var types = GetAvailableTypes(wantedBaseType);
                label = new GUIContent($"Select type for {name}");
                Assert.IsTrue(availableTypes.Any());
                int index = 0;
                while (index < types.Count)
                {
                    var button = new MenuItemTypeList(this, types, ref index);
                    mainMenuItems.Add(button);
                }

                BaseCheatManager.BaseMenuItem.MakeListLoop(mainMenuItems);
            }

            protected override void CheckInputs()
            {
                base.CheckInputs();
                if(Input.GetKeyDown(KeyCode.Escape))
                {
                    state = State.Cancel;
                }
            }

            public override void OnGUI()
            {
                GUILayout.Label("Choose type for parameter " + name);
                BaseCheatManager.Label(label, Size);
                base.OnGUI();
            }

            protected override void OnSubMenuGUI()
            {
                if (MenuItemTypeList.currentList != null && MenuItemTypeList.currentList.HasSubItems)
                {
                    GUILayout.BeginVertical();
                    foreach (var item in MenuItemTypeList.currentList.subMenuItems)
                    {
                        ItemGUI(item);
                    }

                    GUILayout.EndVertical();
                }                
            }

            private static List<Type> availableTypes;

            private static List<Type> GetAvailableTypes(Type baseType)
            {
                availableTypes = new List<Type>();
                AssemblyScanner.Register(type =>
                {
                    if (type.DerivesFrom(baseType)) availableTypes.Add(type);
                });
                AssemblyScanner.Scan(() =>
                {
                    availableTypes.Sort((t1, t2) => string.CompareOrdinal(t1.Name, t2.Name));
                });
                return availableTypes;
            }

            internal void AddParameter(Type type)
            {
                Type filter = typeof(TypeFilter<>).MakeGenericType(wantedBaseType);
                // get the TypeFilter class' constructor that takes System.Type as par paramter
                var c = filter.GetConstructor(new[] {typeof(Type)});
                // create a parameter of Type TypeFilter<type> 
                var t = c.Invoke(new object[] {type});
                builder.AddParameter(t);
            }
        }
    }
}