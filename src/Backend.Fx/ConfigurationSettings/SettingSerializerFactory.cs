namespace Backend.Fx.ConfigurationSettings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using JetBrains.Annotations;

    public class SettingSerializerFactory
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

            throw new ArgumentOutOfRangeException(nameof(T), string.Format("No Serializer for Setting Type {0} available", typeof(T).Name));
        }
    }
}