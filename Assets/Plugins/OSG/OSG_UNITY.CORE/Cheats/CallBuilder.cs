// Old Skull Games
// Bernard Barthelemy
// Thursday, July 6, 2017

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace OSG
{
    public partial class CallBuilder
    {
        protected readonly List<object> gatheredParameters = new List<object>(4);
        protected readonly CheatInfo cheatToExecute;
        private readonly BaseCheatManager manager;
        protected readonly ParameterInfo[] parameterInfos;
        public CallBuilderMenuItemSelector selector;
        public CallBuilder(CheatInfo info, BaseCheatManager manager)
        {
            cheatToExecute = info;
            this.manager = manager;
            parameterInfos = cheatToExecute.method.GetParameters();
        }

        public override string ToString()
        {
            return cheatToExecute.ToString(gatheredParameters);
        }

        public State GatherParametersForCall(Vector2 size)
        {
            int currentParameterIndex = gatheredParameters.Count;
            if (currentParameterIndex >= parameterInfos.Length)
            {
                try{
                    cheatToExecute.Execute(gatheredParameters.ToArray());
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
                return State.Ready;
            }

            if(selector == null)
            {
                selector = GetSelectorForType(parameterInfos[currentParameterIndex]);
            }
            selector.OnGUI();
            switch (selector.state)
            {
                case State.Cancel:  return State.Cancel;
                case State.Ready:  selector = null; break;
            }
            
            return State.Building;
        }


        public void ExecuteAgain()
        {
            cheatToExecute.Execute(gatheredParameters.ToArray());
        }
        public struct TypeFilter<T>
        {
            public Type type;
            public TypeFilter(Type t)
            {
                Assert.IsTrue(t.DerivesFrom(typeof(T)));
                type = t;
            }        
        }

        private string stringParam;

        protected void AddParameter(object parameter)
        {
            gatheredParameters.Add(parameter);
            stringParam = "";
        }

        public delegate bool GUIFunc<T>(Vector2 size, bool validate, out T value);

        private bool IntInput(Vector2 size, bool validate, out int value)
        {
            TextInput(size, true, out stringParam);
            value = 0;
            return validate && Int32.TryParse(stringParam, out value);
        }

        private bool FloatInput(Vector2 size, bool validate, out float param)
        {
            TextInput(size, true, out stringParam);
            param = 0;
            return validate && Single.TryParse(stringParam, out param);
        }

        private bool TextInput(Vector2 size, bool validate, out string param)
        {
            var rect = BaseCheatManager.DrawRect(new GUIContent(stringParam), size);
            stringParam = GUI.TextField(rect, stringParam);
            param = stringParam;
            return validate;
        }

        public enum State
        {
            Building,
            Cancel,
            Ready
        }

        public Vector2 Update()
        {
            return selector?.Update() ?? Vector2.zero;
        }
    }
}