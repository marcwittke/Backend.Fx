﻿namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantActivated : TenantStatusChanged
    {
        public TenantActivated(int tenantId, string name, string description, bool isDemoTenant) 
            : base(tenantId, name, description, isDemoTenant)
        {
        }
    }
}