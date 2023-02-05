// Old Skull Games
// Bernard Barthelemy
// Wednesday, January 16, 2019

using UnityEngine;
using System;

namespace OSG
{
    public class TypeSelectorAttribute : PropertyAttribute
    {
        public Type baseType;

        public TypeSelectorAttribute(Type baseType)
        {
            this.baseType = baseType;
        }
    }
}
