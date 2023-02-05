// Old Skull Games
// Bernard Barthelemy
// Wednesday, November 27, 2019


using System;
using OSG.Services;

namespace OSG.Core.EventSystem
{
    public class EventSystemRef<TContainer, TSystem> : ServiceRef<IEventSystem, TSystem> 
        where TContainer : CoreEventContainer where TSystem : CoreEventSystem
    {
        private TContainer _events;
        public TContainer Events => _events = _events??Value.Get<TContainer>();

        public TContainer RegisterContext(object owner) 
        {
            return Value.RegisterContext<TContainer>(owner);
        }

        public TWanted GetEvents<TWanted>() where TWanted : CoreEventContainer
        {
            return Value.Get<TWanted>();
        }

        public TWanted RegisterContext<TWanted>(object owner) where TWanted : CoreEventContainer
        {
            return Value.RegisterContext<TWanted>(owner);
        }

        public virtual void UnregisterFromAllEvents(object o) => Value.UnregisterFromAllEvents(o);

        public IDisposable SetRegisteringContext(object target)
        {
            return Value.SetRegisteringContext(target);
        }
    }
}