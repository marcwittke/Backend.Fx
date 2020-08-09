﻿namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantCreated : TenantStatusChanged
    {
        public TenantCreated(int tenantId, string name, string description, bool isDemoTenant) 
            : base(tenantId, name, description, isDemoTenant)
        {
        }
    }
}
