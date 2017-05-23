namespace Backend.Fx.Environment.MultiTenancy
{
    using System;
    using System.Diagnostics;
    using BuildingBlocks;

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class TenantId : ValueObject
    {
        private readonly int? id;

        public TenantId(int? id)
        {
            this.id = id;
        }

        /// <summary>
        /// Throws on null id
        /// </summary>
        public int Value
        {
            get
            {
                if (id == null)
                {
                    throw new InvalidOperationException("You must not access tha Value property when the tenant id is null");
                }

                return id.Value;
            }
        }

        public bool HasValue
        {
            get { return id.HasValue; }
        }

        protected string DebuggerDisplay
        {
            get
            {

                if (HasValue)
                {
                    return $"TenantId: {Value}";
                }

                return "TenantId: null";
            }
        }
    }
}