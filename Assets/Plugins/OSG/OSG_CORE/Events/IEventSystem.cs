using System;
using OSG.Services;

namespace OSG.Core.EventSystem
{
    public interface IEventSystem : IService
    {
        void Register(CoreEvent @event, CoreAction action);
        void Register<T>(CoreEvent<T> @event, CoreAction<T> action);
        void RemoveFromAllEvents(object owner);
        void Unregister(CoreEventBase coreEvent, Delegate action);

        TEventContainer RegisterContext<TEventContainer>(object registeringObject) where TEventContainer : CoreEventContainer;
        TEventContainer Get<TEventContainer>() where TEventContainer : CoreEventContainer;
        CoreEventContainer Get(Type containerType);
    }
}
