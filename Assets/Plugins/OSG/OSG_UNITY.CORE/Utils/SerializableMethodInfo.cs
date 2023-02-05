using System;
using System.Reflection;
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Allows to serialize a method info. Just use its Info property to set and get it.
    /// </summary>
    [Serializable]
    public class SerializableMethodInfo
    {
        [SerializeField] private string methodName;
        [SerializeField] private string[] paramsTypesNames;
        [SerializeField] private string declaringTypeName;
        [SerializeField] public int bindingFlags;
   

        private MethodInfo _info;

        public MethodInfo Info
        {
            set
            {
                _info = value;
                declaringTypeName = _info.DeclaringType.AssemblyQualifiedName;
                methodName = _info.Name;
                var parameterTypes = _info.GetParameters();
                int paramCount = parameterTypes.Length;
                paramsTypesNames = new string[paramCount];
                for (int i = 0; i < paramCount; ++i)
                {
                    paramsTypesNames[i] = parameterTypes[i].ParameterType.AssemblyQualifiedName;
                }
            }

            get
            {
                if (_info != null) return _info;
                Type declaringType = Type.GetType(declaringTypeName);
                if (declaringType == null) return null;
                Type[] parameters = new Type[paramsTypesNames.Length];
                for (int i = 0; i < parameters.Length; ++i)
                {
                    parameters[i] = Type.GetType(paramsTypesNames[i]);
                }

                _info = declaringType.GetMethod(methodName, (BindingFlags) bindingFlags, null, parameters, null);
                return _info;
            }
        }
    }
}