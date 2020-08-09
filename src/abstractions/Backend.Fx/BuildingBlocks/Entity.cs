using System;
using System.ComponentModel.DataAnnotations;
using Backend.Fx.Extensions;
using JetBrains.Annotations;

namespace Backend.Fx.BuildingBlocks
{
    /// <summary>
    /// An object that is not defined by its attributes, but rather by a thread of continuity and its identity.
    /// https://en.wikipedia.org/wiki/Domain-driven_design#Building_blocks
    /// </summary>
    public abstract class Entity : Identified
    {
        protected Entity()
        {
        }

        protected Entity(int id)
        {
            Id = id;
        }

        public DateTime CreatedOn { get; protected set; }

        [StringLength(100)] public string CreatedBy { get; protected set; }

        public DateTime? ChangedOn { get; protected set; }

        [StringLength(100)] public string ChangedBy { get; protected set; }

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

            CreatedBy = createdBy.Cut(100);
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

            ChangedBy = changedBy.Cut(100);
            ChangedOn = changedOn;
        }
    }
}