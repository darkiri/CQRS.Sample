using System.Reflection;
using CQRS.Sample.Store;

namespace CQRS.Sample.Aggregates
{
    static internal class ReflectionHelper {
        public static void ApplyEvent1(MutableState mutableState, IEvent evt)
        {
            mutableState.GetType()
                .GetMethod("Apply",
                           BindingFlags.NonPublic | BindingFlags.Instance,
                           null,
                           new[] {evt.GetType()},
                           null)
                .Invoke(mutableState, new object[] {evt});
        }
    }
}