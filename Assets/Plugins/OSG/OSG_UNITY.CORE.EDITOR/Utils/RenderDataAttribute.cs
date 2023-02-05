using System;

namespace OSG
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RenderDataAttribute : Attribute
    {
        private readonly int _priority;
        private readonly string _displayName;

        public RenderDataAttribute(int priority, string displayName)
        {
            _priority = priority;
            _displayName = displayName;
        }

        public int Priority
        {
            get { return _priority; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }
    }
}