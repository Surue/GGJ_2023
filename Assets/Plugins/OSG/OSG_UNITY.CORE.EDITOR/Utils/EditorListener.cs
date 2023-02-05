// Old Skull Games
// Bernard Barthelemy
// Friday, May 19, 2017

using System;
using System.Collections.Generic;
using UnityEditor;

namespace OSG
{
    /// <summary>
    /// Sometimes, for example when a menu popup is opened, or from a GUI function
    /// some editor functionalities don't work properly, and you want them to be
    /// executed when the editor is ready to.
    /// This will do that for you. 
    /// </summary>
    public static class EditorListener
    {
        static List<Action> doActions = new List<Action>();
        /// <summary>
        /// Add an action to execute on next editor update
        /// </summary>
        /// <param name="actionToDo"></param>
        public static void OnNextUpdateDo(Action actionToDo)
        {
            if (doActions.Count == 0)
            {
                EditorApplication.update += DoActions;
            }
            doActions.Add(actionToDo);
        } 

        private static void DoActions()
        {
            try
            {
                List<Action> toDo = doActions;
                doActions = new List<Action>();
                foreach (Action action in toDo)
                {
                    action();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            EditorApplication.update -= DoActions;
        }
    }
}
        
 