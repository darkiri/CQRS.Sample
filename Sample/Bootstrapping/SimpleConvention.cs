using System;
using System.Linq;
using System.Text.RegularExpressions;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace CQRS.Sample.Bootstrapping
{
    public class SimpleConvention : IRegistrationConvention 
    {
        private readonly Regex _lastPascalWord = new Regex(@"[A-Z][^A-Z]+$", RegexOptions.Compiled);
        public void Process(Type type, Registry registry)
        {
            var m = _lastPascalWord.Match(type.Name);
            if (!type.IsAbstract && !type.IsInterface && m.Success)
            {
                var interfaceType = type.GetInterfaces().FirstOrDefault(iface => iface.Name.EndsWith(m.Value));
                if (null != interfaceType)
                {
                    registry.AddType(interfaceType, type);
                }
            }
        }
    }
}