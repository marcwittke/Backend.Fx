using System;
using System.Diagnostics;

namespace Backend.Fx.Features.MultiTenancy
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public readonly struct TenantId
    {
        private readonly int? _id;

        public TenantId(int? id)
        {
            _id = id;
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

        public bool HasValue => _id.HasValue;

        public string DebuggerDisplay
        {
            get
            {
                if (HasValue)
                {
                    return $"TenantId: {Value}";
                }

                return "TenantId: NULL";
            }
        }

        public override string ToString()
        {
            return _id?.ToString() ?? "NULL";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (GetType() != obj.GetType()) return false;
            if (obj is TenantId tenantId)
            {
                if (!HasValue && !tenantId.HasValue)
                {
                    return true;
                }
            
                if (HasValue && tenantId.HasValue)
                {
                    return Equals(Value, tenantId.Value);
                }
            }
            
            return false;
        }

        public override int GetHashCode()
        {
            return HasValue ? Value.GetHashCode() : 487623523.GetHashCode();
        }
        
        public static explicit operator int(TenantId tid) => tid.Value;
        public static explicit operator TenantId(int id) => new(id);
        
        public static bool operator ==(TenantId left, TenantId right)
        {
            if (!left.HasValue && !right.HasValue)
            {
                return true;
            }
            
            if (left.HasValue && right.HasValue)
            {
                return Equals(left.Value, right.Value);
            }
            
            return false;
        }

        public static bool operator !=(TenantId left, TenantId right)
        {
            return !(left == right);
        }
    }
}