using System;
using System.Collections.Generic;
using System.Reflection;

namespace OSG.Core
{
    public static class ReflectionExtensions
    {
        public static T MakeDelegate<T>(this MethodInfo mi) where T : class 
        {
            try
            {
                return Delegate.CreateDelegate(typeof(T), mi) as T;
            }
            catch
            {
                return null;
            }
        }

        public static MemberInfo[] GetMembers(Type type, BindingFlags bindingFlags)
        {
            List<MemberInfo> members = new List<MemberInfo>();
            members.AddRange(type.GetFields(bindingFlags));
            members.AddRange(type.GetProperties(bindingFlags));
            return members.ToArray();
        }
        public static MemberInfo GetMember(Type type, string member, BindingFlags bindingFlags)
        {
            MemberInfo[] memberInfos = type.GetMember(member, bindingFlags);
            // better make sure the member is unique 
            switch (memberInfos.Length)
            {
                case 0: return null;
                case 1: return memberInfos[0];
                default: throw new ArgumentException(type.Name + " has " + memberInfos.Length + " members named '" + member + '\'');
            }
        }

        public static Type GetRealType(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = memberInfo as FieldInfo;
                    return fieldInfo.FieldType;
                case MemberTypes.Method:
                    MethodInfo methodInfo= memberInfo as MethodInfo;
                    return methodInfo.ReturnType;
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                    return propertyInfo.PropertyType;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static object GetValue(this MemberInfo memberInfo, object outer)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = memberInfo as FieldInfo;
                    return fieldInfo.GetValue(outer);
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                    return propertyInfo.GetValue(outer,new object[0]);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void SetValue(this MemberInfo memberInfo, object outer, object value)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = memberInfo as FieldInfo;
                    fieldInfo.SetValue(outer, value);
                    break;
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                    propertyInfo.SetValue(outer, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
