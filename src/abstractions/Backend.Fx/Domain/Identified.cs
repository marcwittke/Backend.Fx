using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Backend.Fx.Domain
{
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public abstract class Identified<TId> : IEquatable<Identified<TId>>
        where TId : struct
    {
        public abstract TId Id { get; protected set; }

        /// <summary>
        /// DON'T USE!
        /// This ctor is only here to allow O/R-Mappers to materialize an object coming from a persistent
        /// store using reflection.
        /// </summary>
        protected Identified()
        {
        }

        protected Identified(TId id)
        {
            Id = id;
        }

        [UsedImplicitly] public string DebuggerDisplay => $"{GetType().Name}[{Id}]";

        public bool Equals(Identified<TId> other)
        {
            if (other == null || other.GetType() != GetType())
            {
                return false;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Identified<TId>;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Identified<TId> x, Identified<TId> y)
        {
            return Equals(x?.Id, y?.Id);
        }

        public static bool operator !=(Identified<TId> x, Identified<TId> y)
        {
            return !(x == y);
        }
    }
}