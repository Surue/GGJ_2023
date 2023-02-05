// Old Skull Games
// Pierre Planeau
// Tuesday, September 26, 2017

using UnityEngine;
using System;
using System.Reflection;

namespace OSG
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ConditionalHideAttribute : PropertyAttribute
    {
        //The name of the bool field that will be in control
        public string ConditionalSourceField = "";
        //TRUE = Hide in inspector / FALSE = Disable in inspector 
        public bool HideInInspector = false;

        public ConditionalHideAttribute(string conditionalSourceField)
        {
            this.ConditionalSourceField = conditionalSourceField;
            this.HideInInspector = false;
        }

        public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector)
        {
            this.ConditionalSourceField = conditionalSourceField;
            this.HideInInspector = hideInInspector;
        }

        public bool PropertyIsVisible(object owner)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo info = owner.GetType().GetField(ConditionalSourceField, bindingFlags);
            return info == null || (bool)info.GetValue(owner);
        }
        
        
    }
}
