using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Backend.Fx.Features.ConfigurationSettings
{
    public interface ISettingSerializerFactory
    {
        ISettingSerializer<T> GetSerializer<T>();
    }

    [PublicAPI]
    public class SettingSerializerFactory : ISettingSerializerFactory
    {
        protected Dictionary<Type, ISettingSerializer> Serializers { get; }

        public SettingSerializerFactory()
        {
            Serializers = typeof(ISettingSerializer)
                          .GetTypeInfo()
                          .Assembly
                          .ExportedTypes
                          .Select(t => t.GetTypeInfo())
                          .Where(t => !t.IsAbstract && t.IsClass && typeof(ISettingSerializer).GetTypeInfo().IsAssignableFrom(t))
                          .ToDictionary(
                              t => t.ImplementedInterfaces
                                    .Single(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ISettingSerializer<>))
                                    .GenericTypeArguments.Single(),
                              t => (ISettingSerializer) Activator.CreateInstance(t.AsType()));
        }

        [NotNull]
        public ISettingSerializer<T> GetSerializer<T>()
        {
            if (Serializers.ContainsKey(typeof(T)))
            {
                return (ISettingSerializer<T>) Serializers[typeof(T)];
            }

            throw new ArgumentOutOfRangeException(nameof(T),
                                                  $"No Serializer for Setting Type {typeof(T).Name} available");
        }
    }
}