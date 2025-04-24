using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Dto.TenantAccount;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.TenantAccount
{
    public interface IRoleCacheService : ICacheService
    {
        /// <summary>
        /// 角色
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<RoleDto>> QueryRole(QueryRoleInput input, Func<Task<PagedResultDto<RoleDto>>> action);
    }
}
