using System;

namespace Backend.Fx.Domain
{
    /// <summary>
    /// The root of an aggregate, identified by an id of type <see cref="TId"/>.
    /// </summary>
    public interface IAggregateRoot<out TId> where TId : struct, IEquatable<TId>
    {
        public TId Id { get; }
    }
}