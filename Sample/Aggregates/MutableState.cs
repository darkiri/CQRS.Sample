using System.Collections.Generic;
using System.Reflection;
using CQRS.Sample.Store;

namespace CQRS.Sample.Aggregates
{
    public abstract class MutableState {
        public void LoadFromHistory(IEnumerable<IEvent> events)
        {
            foreach (var evt in events)
            {
                ApplyEvent(evt);
            }
        }

        public void ApplyEvent(IEvent evt)
        {
            GetType()
                .GetMethod("Apply", BindingFlags.NonPublic | BindingFlags.Instance,
                           null, new[] {evt.GetType()}, null)
                .Invoke(this, new object[] {evt});
        }
    }
}