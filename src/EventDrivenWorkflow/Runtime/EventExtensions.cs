// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventExtensions.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using System.Reflection;

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
        /// A dictionary contains payload getter functions.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<Event, object, Event>> payloadSetters =
            new ConcurrentDictionary<Type, Func<Event, object, Event>>();

        /// <summary>
        /// Get payload of the event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <param name="payloadType">The payload type.</param>
        /// <returns>The payload object.</returns>
        public static object GetPayload(this Event @event, Type payloadType)
        {
            if (payloadType == null)
            {
                return null;
            }

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

        /// <summary>
        /// Create a copy of event with payload set.
        /// </summary>
        /// <param name="event">The source event.</param>
        /// <param name="payloadType">The payload type.</param>
        /// <param name="payload">The payload.</param>
        /// <returns>The copy event with payload.</returns>
        public static Event SetPayload(this Event @event, Type payloadType, object payload)
        {
            if (payloadType == null)
            {
                return @event;
            }

            Func<Event, object, Event> setter;
            if (!payloadSetters.TryGetValue(payloadType, out setter))
            {
                // Compile function: (e, p) =>
                // {
                //   var x = new Event<T>();
                //   x.Id = e.Id;
                //   x.Name = e.Name;
                //   ...
                //   x.Payload = (T)p;
                //   return (Event)x;
                // }
                var eventOfPayloadType = typeof(Event<>).MakeGenericType(payloadType);

                var eParameter = Expression.Parameter(typeof(Event), "e");
                var pParameter = Expression.Parameter(typeof(object), "p");

                var xVariable = Expression.Variable(eventOfPayloadType, "x");
                var eventConstructor = Expression.New(eventOfPayloadType.GetConstructor(Array.Empty<Type>()));

                List<Expression> expressions = new List<Expression>();

                // x = new Event<T>();
                expressions.Add(Expression.Assign(xVariable, eventConstructor));

                foreach (var property in typeof(Event).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!property.CanRead || !property.CanWrite)
                    {
                        continue;
                    }

                    // x.Property = e.Property
                    var xProperty = Expression.Property(xVariable, property);
                    var eProperty = Expression.Property(eParameter, property);
                    expressions.Add(Expression.Assign(xProperty, eProperty));
                }

                // x.Payload = (T)p;
                var convertPayload = Expression.Convert(pParameter, payloadType);
                var payloadProperty = Expression.Property(xVariable, nameof(Event<object>.Payload));
                expressions.Add(Expression.Assign(payloadProperty, convertPayload));

                var convertX = Expression.Convert(xVariable, typeof(Event));

                LabelTarget returnTarget = Expression.Label(typeof(Event));
                GotoExpression returnExpression = Expression.Return(returnTarget, convertX);

                // Note, the defaultValue must be specified if there is a return type defined.
                LabelExpression returnLabel = Expression.Label(returnTarget, defaultValue: Expression.Constant(null, typeof(Event)));

                expressions.Add(returnExpression);
                expressions.Add(returnLabel);

                // Variable definitions must be specified separately from other expressions.
                var block = Expression.Block(new[] { xVariable }, expressions);
                var lamda = Expression.Lambda<Func<Event, object, Event>>(block, eParameter, pParameter);

                setter = lamda.Compile();

                payloadSetters.TryAdd(payloadType, setter);
            }

            return setter.Invoke(@event, payload);
        }
    }
}
