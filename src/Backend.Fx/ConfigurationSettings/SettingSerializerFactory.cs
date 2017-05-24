namespace Backend.Fx.ConfigurationSettings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using JetBrains.Annotations;

    public class SettingSerializerFactory
    {
        private readonly Dictionary<Type, ISettingSerializer> serializers;

        public SettingSerializerFactory()
        {
            serializers = typeof(ISettingSerializer)
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
            if (serializers.ContainsKey(typeof(T)))
            {
                return (ISettingSerializer<T>) serializers[typeof(T)];
            }

            throw new ArgumentOutOfRangeException(nameof(T), string.Format("No Serializer for Setting Type {0} available", typeof(T).Name));
        }
    }
}