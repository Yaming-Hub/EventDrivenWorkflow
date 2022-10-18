using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow;

namespace Microsoft.EventDrivenWorkflow.Core
{
    internal static class EventExtensions
    {
        private static readonly ConcurrentDictionary<Type, Func<Event, object>> payloadGetters = new ConcurrentDictionary<Type, Func<Event, object>>();

        public static object GetPayload(this Event @event, Type payloadType)
        {
            Func<Event, object> getter;
            if (!payloadGetters.TryGetValue(payloadType, out getter))
            {
                // Compile function: (e) => (object)((IEvent<T>)e).Payload;
                var eventOfPayloadType = typeof(Event<>).MakeGenericType(payloadType);
                var payloadProperty = eventOfPayloadType.GetProperty(nameof(Event<object>.Payload));

                var eventParameter = Expression.Parameter(typeof(Event), "e");
                var convertToEventWithPayload = Expression.Convert(eventParameter, eventOfPayloadType);
                var getPayloadProperty = Expression.Property(convertToEventWithPayload, payloadProperty);
                var convertPayloadToObject = Expression.Convert(getPayloadProperty, typeof(object));
                var lamda = Expression.Lambda<Func<Event, object>>(convertPayloadToObject, eventParameter);

                getter = lamda.Compile();

                payloadGetters.TryAdd(payloadType, getter);
            }

            return getter.Invoke(@event);
        }
    }
}
