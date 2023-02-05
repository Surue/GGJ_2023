using System.Reflection;
using UnityEngine;

namespace OSG
{
    public static class MethodInfoExtension
    {
        /// <summary>
        /// Generates a GUIContent describing a method.
        /// The label should be kept short, but the tooltip will as complete as possible
        /// </summary>
        /// <param name="methodInfo">the method to describe</param>
        /// <param name="withDeclaringType">include the name of the type </param>
        /// <returns></returns>
        public static GUIContent GUIContent(this MethodInfo methodInfo, bool withDeclaringType)
        {
            if (methodInfo == null) return new GUIContent("<none>");
            string text = withDeclaringType  && methodInfo.DeclaringType != null
                ? methodInfo.DeclaringType.Name + "." 
                : "";
            text += methodInfo.Name + "(";
            string toolTip = (methodInfo.DeclaringType != null ? methodInfo.DeclaringType.FullName :"") 
                             + "." + methodInfo.Name + "(";
            var parameters = methodInfo.GetParameters();
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (i > 0)
                {
                    text += ", ";
                    toolTip += ", ";
                }
                text += parameters[i].ParameterType.Name;
                toolTip += parameters[i].ParameterType.FullName + " " + parameters[i].Name;
            }
            text += ")";
            toolTip += ")";
            return new GUIContent(text, toolTip);
        }
    }
}