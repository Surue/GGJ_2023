// Old Skull Games
// Bernard Barthelemy
// Tuesday, July 4, 2017

using System;
using UnityEngine;

namespace OSG
{
   [Serializable]
    public class CheatShortcut
    {
        public KeyCode key;
        public string name;

        [HideInInspector] public CheatInfo cheatInfo;

        public string Label()
        {
            return "(" + key + ")";
        }
    }
 }