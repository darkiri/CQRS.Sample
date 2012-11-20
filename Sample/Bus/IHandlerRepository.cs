using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CQRS.Sample.Bus
{
    public interface IHandlerRepository
    {
        IEnumerable<MethodInfo> GetHandlers();
        object GetInstance(MethodInfo handlerDef);
    }

    public class EmptyRepository : IHandlerRepository
    {
        public IEnumerable<MethodInfo> GetHandlers()
        {
            return Enumerable.Empty<MethodInfo>();
        }

        public object GetInstance(MethodInfo handlerDef)
        {
            return null;
        }
    }
}