// Old Skull Games
// Bernard Barthelemy
// Thursday, March 5, 2020

using System;
using System.Reflection;
using UnityEngine;

namespace OSG
{
    public partial class CallBuilder
    {
        public abstract class CallBuilderMenuItemSelector : BaseCheatManager.BaseMenuItemSelector
        {
            public State state;
            public readonly CallBuilder builder;
            protected readonly string name;

            protected CallBuilderMenuItemSelector(CallBuilder builder, string name) : base(builder.manager)
            {
                this.builder = builder;
                this.name = name;
                state = State.Building;
            }
        }

        protected virtual CallBuilderMenuItemSelector GetSelectorForType(ParameterInfo parameterInfo)
        {
            Type paramType = parameterInfo.ParameterType;
            string name = parameterInfo.Name;

            if (paramType == typeof(int))
            {
                return new CallBuilderSimpleTypeSelector<int>(this, IntInput, name);
            }

            if (paramType == typeof(string))
            {
                return new CallBuilderSimpleTypeSelector<string>(this, TextInput, name);
            }

            if (paramType == typeof(float))
            {
                return new CallBuilderSimpleTypeSelector<float>(this, FloatInput, name);
            }

            if (paramType.Name.Contains("TypeFilter"))
            {
                return new CallBuilderTypeSelector(this, name, paramType.GenericTypeArguments[0]);
            }

            Debug.LogError("I don't how to get a parameter of type " + paramType.Name);
            return null;
        }
    }
}