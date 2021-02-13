using System;
using System.Linq;
using System.Reflection;

namespace Backend.Fx.Hacking
{
    public static class PrivateUtil
    {
        public static T CreateInstanceFromPrivateDefaultConstructor<T>()
        {
            ConstructorInfo constructor = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).SingleOrDefault(ci => ci.GetParameters().Length == 0);
            if (constructor == null)
            {
                throw new InvalidOperationException($"No private default constructor found in {typeof(T).Name}");
            }

            var instance = (T) constructor.Invoke(null);
            return instance;
        }
    }
}