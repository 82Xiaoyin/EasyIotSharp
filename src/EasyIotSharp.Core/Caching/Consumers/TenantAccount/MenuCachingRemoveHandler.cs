using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Tenant.Impl;
using EasyIotSharp.Core.Caching.Tenant;
using EasyIotSharp.Core.Events.Tenant;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;
using EasyIotSharp.Core.Caching.TenantAccount;
using EasyIotSharp.Core.Events.TenantAccount;
using EasyIotSharp.Core.Caching.TenantAccount.Impl;

namespace EasyIotSharp.Core.Caching.Consumers.TenantAccount
{
    public class MenuCachingRemoveHandler : IEventHandler<MenuEventData>, ITransientDependency
    {
        private readonly IMenuCacheService _menuCacheService;
        public MenuCachingRemoveHandler(IMenuCacheService menuCacheService)
        {
            _menuCacheService = menuCacheService;
        }
        public void HandleEvent(MenuEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _menuCacheService.Cache.RemoveAsync(MenuCacheService.KEY_TENANTACCOUNT_MENU_QUERY.FormatWith(i, 10));
            }
        }
    }
}
