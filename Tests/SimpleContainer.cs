using System;
using System.Collections.Generic;

namespace CQRS.Sample.Tests
{
    //public class SimpleContainer : IContainer
    //{
    //    private Dictionary<Type, object> _objects = new Dictionary<Type, object>();
    //    public T GetInstance<T>()
    //    {
    //        return (T)GetInstance(typeof(T));
    //    }

    //    public object GetInstance(Type t)
    //    {
    //        if (!_objects.ContainsKey(t))
    //        {
    //            _objects[t] = CreateWithDefaultConstructor(t);
    //        }
    //        return _objects[t];
    //    }

    //    private static object CreateWithDefaultConstructor(Type t)
    //    {
    //        return t.GetConstructor(new Type[0]).Invoke(new object[0]);
    //    }
    //}
}