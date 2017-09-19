namespace Backend.Fx.BuildingBlocks
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using JetBrains.Annotations;

    /// <summary>
    /// An object that is not defined by its attributes, but rather by a thread of continuity and its identity.
    /// https://en.wikipedia.org/wiki/Domain-driven_design#Building_blocks
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public abstract class Entity : IEquatable<Entity>
    {
        protected Entity()
        { }

        protected Entity(int id)
        {
            Id = id;
        }

        [UsedImplicitly]
        public string DebuggerDisplay
        {
            get { return string.Format("{0}[{1}]", GetType().Name, Id); }
        }

        [Key]
        public int Id { get; private set; }

        public DateTime CreatedOn { get; protected set; }

        [StringLength(100)]
        public string CreatedBy { get; protected set; }

        public DateTime? ChangedOn { get; protected set; }

        [StringLength(100)]
        public string ChangedBy { get; protected set; }

        public void SetCreatedProperties([NotNull] string createdBy, DateTime createdOn)
        {
            if (createdBy == null)
            {
                throw new ArgumentNullException(nameof(createdBy));
            }
            if (createdBy == string.Empty)
            {
                throw new ArgumentException(nameof(createdBy));
            }
            CreatedBy = createdBy.Length > 100 ? createdBy.Substring(0, 99) + "…" : createdBy;
            CreatedOn = createdOn;
        }

        public void SetModifiedProperties([NotNull] string changedBy, DateTime changedOn)
        {
            if (changedBy == null)
            {
                throw new ArgumentNullException(nameof(changedBy));
            }
            if (changedBy == string.Empty)
            {
                throw new ArgumentException(nameof(changedBy));
            }
            ChangedBy = changedBy.Length > 100 ? changedBy.Substring(0, 99) + "…" : changedBy;
            ChangedOn = changedOn;

            // Modifying me results implicitly in a modification of the aggregate root, too.
            AggregateRoot myAggregateRoot = FindMyAggregateRoot();
            if (myAggregateRoot != this)
            {
                myAggregateRoot?.SetModifiedProperties(changedBy, changedOn);
            }
        }

        public void SetDeleted([NotNull] string changedBy, DateTime changedOn)
        {
            if (changedBy == null)
            {
                throw new ArgumentNullException(nameof(changedBy));
            }
            if (changedBy == string.Empty)
            {
                throw new ArgumentException(nameof(changedBy));
            }
            
            // Deleting me results implicitly in a modification of the aggregate root.
            AggregateRoot myAggregateRoot = FindMyAggregateRoot();
            if (myAggregateRoot != this)
            {
                myAggregateRoot?.SetModifiedProperties(changedBy, changedOn);
            }
        }

        protected abstract AggregateRoot FindMyAggregateRoot();

        public bool Equals(Entity other)
        {
            if (other == null)
            {
                return false;
            }

            return other.GetType() == GetType() && Id.Equals(other.Id);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object"></see> is equal to the current
        ///     <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.
        /// </param>
        /// <returns>
        ///     true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>
        ///     ; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as Entity;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        /// <summary>
        ///     Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use
        ///     in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        ///     A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            // id is practically readonly, only for framework reasons it can be set (because of EF, mostly)
            return Id == default(int)
                ? base.GetHashCode()
                : Id.GetHashCode();
        }

        /// <summary>
        ///     Equality operator so we can have == semantics
        /// </summary>
        public static bool operator ==(Entity x, Entity y)
        {
            return Equals(x, y);
        }

        /// <summary>
        ///     Inequality operator so we can have != semantics
        /// </summary>
        public static bool operator !=(Entity x, Entity y)
        {
            return !(x == y);
        }
    }
}
