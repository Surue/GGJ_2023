using System;

namespace OSG
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheatFunctionAttribute : Attribute
    {
        public readonly CheatCategory category;
        public readonly bool autoClose;
        public string CustomMenuDisplay;

        public CheatFunctionAttribute(CheatCategory cat, bool autoClose)
        {
            category = cat;
            this.autoClose=autoClose;
        }
    }
}