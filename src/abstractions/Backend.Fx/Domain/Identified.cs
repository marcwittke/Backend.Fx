using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Backend.Fx.Domain
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public abstract class Identified : IEquatable<Identified>
    {
        [Key] public int Id { get; set; }

        [UsedImplicitly] public string DebuggerDisplay => $"{GetType().Name}[{Id}]";

        public bool Equals(Identified other)
        {
            if (other == null || other.GetType() != GetType())
            {
                return false;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Identified;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            if (Id != 0)
            {
                return Id.GetHashCode();
            }
            // ReSharper enable NonReadonlyMemberInGetHashCode

            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }

        public static bool operator ==(Identified x, Identified y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(Identified x, Identified y)
        {
            return !(x == y);
        }
    }
}