using System;

namespace OSG
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SortDataAttribute : Attribute
    {
        private readonly string _displayName;

        public SortDataAttribute(string displayName)
        {
            _displayName = displayName;
        }

        public string DisplayName
        {
            get { return _displayName; }
        }
    }
}