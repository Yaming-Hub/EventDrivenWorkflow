// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventExtensions.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;

    /// <summary>
    /// This class defines extension methods of the <see cref="Event"/> class.
    /// </summary>
    internal static class EventExtensions
    {
        /// <summary>
        /// A dictionary contains payload getter functions.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<Event, object>> payloadGetters =
            new ConcurrentDictionary<Type, Func<Event, object>>();

        /// <summary>
        /// Get payload of the event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <param name="payloadType">The payload type.</param>
        /// <returns>The payload object.</returns>
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
