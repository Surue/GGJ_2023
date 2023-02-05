// Old Skull Games
// Pierre Planeau
// Monday, May 21, 2018


using System;

namespace OSG.Core.EventSystem
{
    /// <summary>
    /// Base class for an eventContainer
    /// DON'T ADD any event in there
    /// it implements the IDisposable interface, to allow the using(var events = eventSystem.RegisterContext(this)) construct,
    /// which will allow to not forget to add a clumsy eventSystem.EndRegistering 
    /// </summary>
    public abstract class CoreEventContainer : IDisposable
    {
        private CoreEventSystem coreEventSystem;

        public void Dispose()
        {
            coreEventSystem.EndRegistering();
        }

        public void SetSystem(CoreEventSystem eventSystem)
        {
            coreEventSystem = eventSystem;
            EventContainerHelper.SetSystemForAllEventsInContainer(eventSystem, this);
        }
    }
}
