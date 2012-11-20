using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StructureMap;

namespace CQRS.Sample.Bus
{
    public class AssemblyHandlerRepository : IHandlerRepository
    {
        readonly Assembly _assembly;

        public AssemblyHandlerRepository(Assembly assembly)
        {
            _assembly = assembly;
        }


        public IEnumerable<MethodInfo> GetHandlers()
        {
            return _assembly
                .GetTypes()
                .Where(t => !t.IsAbstract)
                .SelectMany(MessageHandlersIn);
        }

        public object GetInstance(MethodInfo handlerDef)
        {
            return ObjectFactory.GetInstance(handlerDef.ReflectedType);
        }

        public static IEnumerable<MethodInfo> MessageHandlersIn(Type t)
        {
            return t.GetMethods()
                .Where(m => m.IsPublic && m.Name == "Handle")
                .Where(m => m.GetParameters().Count() == 1)
                .Where(HasMessageParameter);
        }

        static bool HasMessageParameter(MethodInfo m)
        {
            return typeof (IMessage).IsAssignableFrom(m.GetParameters().First().ParameterType);
        }
    }
}