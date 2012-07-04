using System;
using StructureMap;

namespace CQRS.Sample
{
    public interface IContainer
    {
        T GetInstance<T>();
        object GetInstance(Type t);
    }

    public class StructureMapContainer : IContainer
    {
        public T GetInstance<T>()
        {
            return ObjectFactory.GetInstance<T>();
        }

        public object GetInstance(Type t)
        {
            return ObjectFactory.GetInstance(t);
        }
    }
}