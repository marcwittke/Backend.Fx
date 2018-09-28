using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Fx.BuildingBlocks
{
    public abstract class Identitfied : IEquatable<Identitfied>
    {
        [Key]
        public int Id { get; set; }


        public bool Equals(Identitfied other)
        {
            if (other == null || !(other.GetType() == GetType()))
                return false;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            Identitfied other = obj as Identitfied;
            if (other == null)
                return false;
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

        public static bool operator ==(Identitfied x, Identitfied y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(Identitfied x, Identitfied y)
        {
            return !(x == y);
        }
    }
}
