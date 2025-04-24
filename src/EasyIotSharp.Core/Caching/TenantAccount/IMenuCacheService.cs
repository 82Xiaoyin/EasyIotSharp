using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Dto.TenantAccount;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.TenantAccount
{
    public interface IMenuCacheService : ICacheService
    {
        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<MenuTreeDto>> QueryMenu(QueryMenuInput input, Func<Task<PagedResultDto<MenuTreeDto>>> action);
    }
}
