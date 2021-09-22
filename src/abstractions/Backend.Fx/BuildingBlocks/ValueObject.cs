using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Fx.BuildingBlocks
{
    /// <summary>
    ///     An object that contains attributes but has no conceptual identity.
    ///     https://en.wikipedia.org/wiki/Domain-driven_design#Building_blocks
    /// </summary>
    public abstract class ValueObject
    {
        /// <summary>
        ///     When overriden in a derived class, returns all components of a value objects which constitute its identity.
        /// </summary>
        /// <returns>An ordered list of equality components.</returns>
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (GetType() != obj.GetType())
            {
                return false;
            }

            return GetEqualityComponents().SequenceEqual(((ValueObject)obj).GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                foreach (object obj in GetEqualityComponents())
                {
                    hash = hash * 23 + (obj != null ? obj.GetHashCode() : 0);
                }

                return hash;
            }
        }
    }


    public abstract class ComparableValueObject : ValueObject, IComparable
    {
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (GetType() != obj.GetType())
            {
                throw new InvalidOperationException();
            }

            return CompareTo(obj as ComparableValueObject);
        }

        protected abstract IEnumerable<IComparable> GetComparableComponents();

        protected int CompareTo(ComparableValueObject other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            using (IEnumerator<IComparable> thisComponents = GetComparableComponents().GetEnumerator())
            {
                using (IEnumerator<IComparable> otherComponents = other.GetComparableComponents().GetEnumerator())
                {
                    while (true)
                    {
                        bool x = thisComponents.MoveNext();
                        bool y = otherComponents.MoveNext();
                        if (x != y)
                        {
                            throw new InvalidOperationException();
                        }

                        if (x)
                        {
                            int c = thisComponents.Current?.CompareTo(otherComponents.Current) ?? 0;
                            if (c != 0)
                            {
                                return c;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    return 0;
                }
            }
        }
    }


    public abstract class ComparableValueObject<T> : ComparableValueObject, IComparable<T>
        where T : ComparableValueObject<T>
    {
        public int CompareTo(T other)
        {
            return base.CompareTo(other);
        }
    }
}
