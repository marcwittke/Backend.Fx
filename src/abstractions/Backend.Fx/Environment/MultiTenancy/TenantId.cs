namespace Backend.Fx.Environment.MultiTenancy
{
    using System;
    using System.Diagnostics;
    using BuildingBlocks;

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class TenantId : ValueObject
    {
        private readonly int? _id;

        public TenantId(int? id)
        {
            this._id = id;
        }

        /// <summary>
        /// Throws on null id
        /// </summary>
        public int Value
        {
            get
            {
                if (_id == null)
                {
                    throw new InvalidOperationException("You must not access the Value property when the tenant id is null");
                }

                return _id.Value;
            }
        }

        public bool HasValue
        {
            get { return _id.HasValue; }
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