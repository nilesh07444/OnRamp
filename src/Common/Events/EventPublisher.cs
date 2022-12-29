using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Events
{
    public class EventPublisher : IEventPublisher
    {
        public void Publish<T>(T @event) where T : IEvent
        {
            var eventHandlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
            var eventHandlers = ServiceLocator.Current.GetAllInstances(eventHandlerType);
            foreach (var eventHandler in eventHandlers)
            {
                try
                {
                    ((IEventHandler<T>)eventHandler).Handle(@event);
                }
                catch (InvalidCastException) { }
            }
        }
    }
    public interface IEventPublisher
    {
        void Publish<T>(T @event) where T : IEvent;
    }
}