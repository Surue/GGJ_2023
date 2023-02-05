// Old Skull Games
// Bernard Barthelemy
// Thursday, March 5, 2020


using System.Collections.Generic;
using UnityEngine;

namespace OSG
{
    public partial class BaseCheatManager
    {
        public static void DoNothing()
        {
        }

        public abstract class BaseMenuItemSelector
        {
            public readonly BaseCheatManager manager;
            protected List<BaseMenuItem> mainMenuItems;
            protected BaseMenuItem focusedItem;
            protected BaseMenuItem selectedItem;
            protected Vector2 Size => manager.menuItemSizeInInch;

            protected BaseMenuItemSelector(BaseCheatManager manager)
            {
                this.manager = manager;
            }

            protected Vector2 moveDirection;
            protected Vector2 scrollDirection;
            protected bool enter;

            protected virtual void CheckInputs()
            {
                moveDirection = Vector2.zero;
                scrollDirection = Vector2.zero;


                enter = Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyUp(KeyCode.Return);
                if (!Input.anyKeyDown && !Input.anyKey)
                    return;


                if(Input.GetKey(KeyCode.LeftShift))
                {
                    if(Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        scrollDirection += Vector2.left;
                    }
                    if(Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        scrollDirection += Vector2.right;
                    }
                    if(Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        scrollDirection -= Vector2.up;
                    }
                    if(Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        scrollDirection -= Vector2.down;
                    }

                    scrollDirection *= Screen.dpi * manager.menuItemSizeInInch;
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        moveDirection += Vector2.left;
                    }

                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        moveDirection += Vector2.right;
                    }

                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        moveDirection += Vector2.up;
                    }

                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        moveDirection += Vector2.down;
                    }
                }

                
            }

            public virtual void OnGUI()
            {
                if(mainMenuItems == null || mainMenuItems.Count<=0)
                    return;
                if(focusedItem==null)
                {
                    ChangeFocusTo(mainMenuItems[0]);
                }
                
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                foreach (var item in mainMenuItems)
                {
                    ItemGUI(item);
                }

                GUILayout.EndVertical();

                OnSubMenuGUI();

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            protected virtual void OnSubMenuGUI()
            {
                
            }

            protected virtual void ItemGUI(BaseMenuItem item)
            {
                GUI.color = GetColor(item);
                if (item.OnGUI())
                {
                    selectedItem = item;
                }
                GUI.color = Color.white;
            }

            protected Color GetColor(BaseMenuItem item)
            {
                if (selectedItem == item)
                    return Color.green;
                return focusedItem == item ? Color.yellow : Color.white;
            }

            public virtual void ChangeFocusTo(BaseMenuItem newItem)
            {
                if (newItem != null)
                {
                    focusedItem = newItem;
                    focusedItem.Focus();
                    scrollDirection = Vector2.zero;

                    Rect currentPosition = focusedItem.CurrentPosition;
                    currentPosition.position += manager.cheatSettings.menuItemPosInInch;
                    float minY = currentPosition.yMin;
                    if(minY < 0)
                    {
                        scrollDirection.y += minY;
                    }

                    float maxY = currentPosition.yMax - Screen.height;
                    if(maxY > 0)
                    {
                        scrollDirection.y += maxY;
                    }

                    float minX = currentPosition.xMin;
                    if(minX<0)
                    {
                        scrollDirection.x += minX;
                    }

                    float maxX = currentPosition.xMax - Screen.width;
                    if(maxX > 0)
                    {
                        scrollDirection.x += maxX;
                    }
                }
            }

            public virtual void ChangeSelectionTo(BaseMenuItem newItem)
            {
                selectedItem = newItem;
            }

            public virtual Vector2 Update()
            {
                if (mainMenuItems == null || mainMenuItems.Count <= 0)
                    return scrollDirection;

                CheckInputs();

                if (focusedItem == null)
                    ChangeFocusTo(mainMenuItems[0]);

                if (moveDirection.y < -0.1f)
                {
                    moveDirection.y = 0;
                    ChangeFocusTo(focusedItem.downItem);
                }
                else if (moveDirection.y > 0.1f)
                {
                    moveDirection.y = 0;
                    ChangeFocusTo(focusedItem.upItem);
                }

                if (enter)
                {
                    enter = false;
                    focusedItem.Click();
                }

                if (moveDirection.x > 0.1f)
                {
                    ChangeFocusTo(focusedItem.rightItem);
                }

                if (moveDirection.x < -0.1f)
                {
                    ChangeFocusTo(focusedItem.leftItem);
                }

                return scrollDirection;
            }

        }
    }
}