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
    public class RoleCachingRemoveHandler : IEventHandler<RoleEventData>, ITransientDependency
    {
        private readonly IRoleCacheService _roleCacheService;
        public RoleCachingRemoveHandler(IRoleCacheService roleCacheService)
        {
            _roleCacheService = roleCacheService;
        }
        public void HandleEvent(RoleEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _roleCacheService.Cache.RemoveAsync(RoleCacheService.KEY_TENANTACCOUNT_ROLE_QUERY.FormatWith(i, 10));
            }
        }
    }
}
