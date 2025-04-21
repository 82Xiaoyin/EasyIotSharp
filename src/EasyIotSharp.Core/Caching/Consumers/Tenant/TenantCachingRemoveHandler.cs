using EasyIotSharp.Core.Caching.Tenant;
using EasyIotSharp.Core.Caching.Tenant.Impl;
using EasyIotSharp.Core.Events.Tenant;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using UPrime.Events.Bus.Handlers;

namespace EasyIotSharp.Core.Caching.Consumers.Tenant
{
    public class TenantCachingRemoveHandler : IEventHandler<TenantEventData>
    {
        private readonly ITenantCacheService _tenantCacheService;
        public TenantCachingRemoveHandler(ITenantCacheService tenantCacheService)
        {
            _tenantCacheService = tenantCacheService;
        }
        public void HandleEvent(TenantEventData eventData)
        {
            if (eventData == null) return;
            for (int i = 1; i <= 5; i++)
            {
                _tenantCacheService.Cache.RemoveAsync(TenantCacheService.KEY_TENANT_QUERY.FormatWith(i, 10));
            }
        }
    }
}
