using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyIotSharp.Core.Dto.Tenant;
using EasyIotSharp.Core.Dto.Tenant.Params;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Tenant
{
    public interface ITenantService
    {

        /// <summary>
        /// 获取所有租户
        /// </summary>
        /// <returns></returns>
        Task<List<EasyIotSharp.Core.Domain.Tenant.Tenant>> GetTenantList();
        /// <summary>
        /// 通过租户id获取一条租户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TenantDto> GetTenant(string id);

        /// <summary>
        /// 通过条件分页查询租户列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<TenantDto>> QueryTenant(QueryTenantInput input);

        /// <summary>
        /// 添加一个租户信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task InsertTenant(InsertTenantInput input);

        /// <summary>
        /// 通过id修改一个租户信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateTenant(UpdateTenantInput input);

        /// <summary>
        /// 通过id修改一个租户的冻结状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateIsFreezeTenant(UpdateIsFreezeTenantTenantInput input);

        /// <summary>
        /// 通过id删除一个租户信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteTenant(DeleteTenantInput input);
    }
}
