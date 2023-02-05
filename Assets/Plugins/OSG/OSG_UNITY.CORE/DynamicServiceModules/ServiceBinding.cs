// Old Skull Games
// Bernard Barthelemy
// Thursday, November 7, 2019
    
using System;

namespace OSG.Services
{
    [Serializable]
    public class SerializedServiceInterface : SerializedType
    {
        public override Type baseType => effectiveType;
    }

    [Serializable]
    public class SerializedService : SerializedType<object>
    {

    }


    [Serializable]
    public class ServiceBinding
    {
        public SerializedServiceInterface serviceInterface;
        public SerializedService serviceType;

        public ServiceBinding()
        {

        }

        public ServiceBinding(Type instanceType, Type interfaceType)
        {
            serviceInterface = new SerializedServiceInterface();
            serviceInterface.SetEffectiveType(interfaceType);
            serviceType = new SerializedService();
            serviceType.SetEffectiveType(instanceType);
        }
    }
}