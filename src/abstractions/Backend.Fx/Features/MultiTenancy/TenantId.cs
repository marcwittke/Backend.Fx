﻿using System;
using System.Diagnostics;

namespace Backend.Fx.Features.MultiTenancy
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class TenantId
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

        public override string ToString()
        {
            return _id?.ToString() ?? "NULL";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(null, obj)) return false;
            if (GetType() != obj.GetType()) return false;
            return Equals(Value, (obj as TenantId)?.Value);
        }

        public override int GetHashCode()
        {
            return HasValue ? Value.GetHashCode() : 17.GetHashCode();
        }
        
        public static explicit operator int(TenantId tid) => tid.Value;
        public static explicit operator TenantId(int id) => new TenantId(id);
    }
}