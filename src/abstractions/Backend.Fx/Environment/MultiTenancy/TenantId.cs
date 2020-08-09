﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.Environment.MultiTenancy
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class TenantId : ValueObject
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

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return _id;
        }
    }
}