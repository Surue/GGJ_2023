// Old Skull Games
// Bernard Barthelemy
// Monday, October 28, 2019


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OSG.Core.EventSystem;

namespace OSG.EventSystem
{
    public class EventReferenceAttribute : ConditionalHideFunctionAttribute
    {
        private readonly Type[] availableTypes;
        public EventReferenceAttribute(params Type[] payloadTypes) : this("", payloadTypes)
        {
            availableTypes = payloadTypes;
        }

        public EventReferenceAttribute(string method, params Type[] payloadTypes) : base(method, true)
        {
            availableTypes = payloadTypes;
        }

        private List<FieldInfo> eventNames;
        public List<FieldInfo> EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new List<FieldInfo>();
                    if (availableTypes != null && availableTypes.Length > 0)
                    {
                        foreach (Type type in availableTypes)
                        {
                            eventNames.AddRange(EventContainerHelper.GetEventsWithPayloadType(type));
                        }
                    }
                    else
                    {
                        eventNames = EventContainerHelper.GetAllEventInfos().ToList();
                    }
                    eventNames.Sort((e1,e2)=>string.CompareOrdinal(e1.Name, e2.Name));
                }
                return eventNames;
            }
        }

    }
}