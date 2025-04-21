using EasyIotSharp.Core.Caching.Tenant;
using EasyIotSharp.Core.Caching.Tenant.Impl;
using EasyIotSharp.Core.Events.Tenant;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;
using UPrime.Events.Bus;
namespace EasyIotSharp.Core.Caching.Consumers.Tenant
{
    public class TenantCachingRemoveHandler : IEventHandler<TenantEventData>, ITransientDependency
    {
        private readonly ITenantCacheService _tenantCacheService;
        public TenantCachingRemoveHandler(ITenantCacheService tenantCacheService)
        {
            _tenantCacheService = tenantCacheService;
        }
        public void HandleEvent(TenantEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _tenantCacheService.Cache.RemoveAsync(TenantCacheService.KEY_TENANT_QUERY.FormatWith(i, 10));
            }
        }
    }
}
