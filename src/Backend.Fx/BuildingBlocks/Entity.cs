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
    public abstract class Entity
    {
        [UsedImplicitly]
        public string DebuggerDisplay
        {
            get { return string.Format("{0}[{1}]", GetType().Name, Id); }
        }

        [Key]
        public int Id { get; set; }

        public DateTime CreatedOn { get; protected set; }

        [StringLength(100)]
        public string CreatedBy { get; protected set; }

        public DateTime? ChangedOn { get; protected set; }

        [StringLength(100)]
        public string ChangedBy { get; protected set; }

        public int Version { get; protected set; }

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
            Version = 1;
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
            Version++;

            // Modifying me results implicitly in a modification of the aggregate root, too.
            AggregateRoot myAggregateRoot = FindMyAggregateRoot();
            if (myAggregateRoot != this)
            {
                myAggregateRoot?.SetModifiedProperties(changedBy, changedOn);
            }
        }

        protected abstract AggregateRoot FindMyAggregateRoot();
    }
}
