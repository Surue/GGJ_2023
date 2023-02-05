// Old Skull Games

using System;

namespace OSG.Core
{
   public abstract class BaseParam
    {
        public readonly Type stateType;

        protected BaseParam(Type t)
        {
            stateType = t;
        }
        
        
        public sealed override bool Equals(object obj)
        {
            BaseParam param = obj as BaseParam;
            if(param == null)
                return false;
            return GetType() == param.GetType() && Equals(param);
        }

        private bool Equals(BaseParam other)
        {
            haveSameParamsCalled = false;
            bool equals = HaveSameParams(other);
            if (!haveSameParamsCalled)
            {
                throw new Exception($" overrides of {nameof(HaveSameParams)} MUST call their base method");
            }

            return equals;
        }

        public override int GetHashCode()
        {
            return (stateType != null ? stateType.GetHashCode() : 0);
        }

        private bool haveSameParamsCalled;
        protected virtual bool HaveSameParams(BaseParam baseParam)
        {
            haveSameParamsCalled = true;
            return baseParam.stateType == stateType;
        }

        public override string ToString()
        {
            return GetHashCode().ToString("X") + " " + GetType().Name + " " + stateType.Name;
        }
    }
}
