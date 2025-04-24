using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.TenantAccount.Impl;
using EasyIotSharp.Core.Caching.TenantAccount;
using EasyIotSharp.Core.Events.TenantAccount;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;

namespace EasyIotSharp.Core.Caching.Consumers.TenantAccount
{
    public class SoldierCachingRemoveHandler : IEventHandler<SoldierEventData>, ITransientDependency
    {
        private readonly ISoldierCacheService _soldierCacheService;
        public SoldierCachingRemoveHandler(ISoldierCacheService soldierCacheService)
        {
            _soldierCacheService = soldierCacheService;
        }
        public void HandleEvent(SoldierEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _soldierCacheService.Cache.RemoveAsync(SoldierCacheService.KEY_TENANTACCOUNT_SOLDIER_QUERY.FormatWith(i, 10));
            }
        }
    }
}
