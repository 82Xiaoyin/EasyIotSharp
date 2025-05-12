using EasyIotSharp.Core.Repositories.Tenant;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyIotSharp.Core.Dto.Tenant.Params;
using EasyIotSharp.Core.Dto.Tenant;
using UPrime.Services.Dto;
using UPrime.AutoMapper;
using EasyIotSharp.Core.Repositories.TenantAccount;
using EasyIotSharp.Core.Services.TenantAccount;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Caching.Tenant;
using EasyIotSharp.Core.Events.Tenant;

namespace EasyIotSharp.Core.Services.Tenant.Impl
{
    /// <summary>
    /// 租户服务实现类
    /// </summary>
    public class TenantService : ServiceBase, ITenantService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly ISoldierService _soldierService;
        private readonly ITenantCacheService _tenantCacheService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tenantRepository">租户仓储</param>
        /// <param name="soldierService">用户服务</param>
        /// <param name="tenantCacheService">租户缓存服务</param>
        public TenantService(ITenantRepository tenantRepository,
                             ISoldierService soldierService,
                             ITenantCacheService tenantCacheService)
        {
            _tenantRepository = tenantRepository;
            _soldierService = soldierService;
            _tenantCacheService = tenantCacheService;
        }

        /// <summary>
        /// 获取所有租户
        /// </summary>
        /// <returns></returns>
        public async Task<List<EasyIotSharp.Core.Domain.Tenant.Tenant>> GetTenantList()
        {
            return await _tenantRepository.GetTenantList();
        }

        /// <summary>
        /// 获取单个租户信息
        /// </summary>
        /// <param name="id">租户ID</param>
        /// <returns>租户信息</returns>
        public async Task<TenantDto> GetTenant(string id)
        {
            var info = await _tenantRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            return info.MapTo<TenantDto>();
        }

        /// <summary>
        /// 查询租户列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页后的租户列表</returns>
        /// <remarks>
        /// 当满足以下条件时使用缓存：
        /// 1. 无关键字搜索
        /// 2. 过期类型为-1
        /// 3. 无合同开始和结束时间筛选
        /// 4. 使用分页且在前5页
        /// 5. 每页大小为10
        /// </remarks>
        public async Task<PagedResultDto<TenantDto>> QueryTenant(QueryTenantInput input)
        {
            if (string.IsNullOrEmpty(input.Keyword) && input.ExpiredType.Equals(-1)
                && input.ContractEndTime.IsNull() &&
                input.ContractStartTime.IsNull()
                && input.IsPage.Equals(true)
                && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _tenantCacheService.QueryTenant(input, async () =>
                {
                    var query = await _tenantRepository.Query(input.Keyword, input.ExpiredType, input.ContractStartTime, input.ContractEndTime, input.IsFreeze, input.PageIndex, input.PageSize);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<TenantDto>>();
                    return new PagedResultDto<TenantDto>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _tenantRepository.Query(input.Keyword, input.ExpiredType, input.ContractStartTime, input.ContractEndTime, input.IsFreeze, input.PageIndex, input.PageSize);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<TenantDto>>();
                return new PagedResultDto<TenantDto>() { TotalCount = totalCount, Items = list };
            }

        }

        /// <summary>
        /// 新增租户
        /// </summary>
        /// <param name="input">新增参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">当租户名称重复时抛出异常</exception>
        /// <remarks>
        /// 执行操作：
        /// 1. 检查租户名称是否重复
        /// 2. 生成租户编号
        /// 3. 创建租户管理员账号
        /// 4. 创建租户记录
        /// 5. 清除相关缓存
        /// </remarks>
        public async Task InsertTenant(InsertTenantInput input)
        {
            var info = await _tenantRepository.FirstOrDefaultAsync(x => x.Name == input.Name && x.IsDelete == false);
            if (info.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "租户名称重复");
            }
            int numId = (await _tenantRepository.CountAsync()) + 1;
            //创建租户管理员账号
            string managerId = await _soldierService.InsertAdminSoldier(new InsertAdminSoldierInput()
            {
                Mobile = input.Mobile,
                Username = input.Owner,
                IsTest = false,
                Sex = -1,
                Email = "",
            });


            var model = new EasyIotSharp.Core.Domain.Tenant.Tenant();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.NumId = numId;
            model.Name = input.Name;
            model.StoreKey = "";
            model.ContractName = input.ContractName;
            model.ContractOwnerName = input.ContractOwnerName;
            model.ContractOwnerMobile = input.ContractOwnerMobile;
            model.ContractStartTime = input.ContractStartTime;
            model.ContractEndTime = input.ContractEndTime;
            model.Owner = input.Owner;
            model.Mobile = input.Mobile;
            model.StoreLogoUrl = input.StoreLogoUrl;
            model.Remark = input.Remark;
            model.Email = input.Email;
            model.VersionTypeId = input.VersionTypeId;
            model.IsFreeze = input.IsFreeze;
            model.FreezeDes = input.FreezeDes;
            model.ManagerId = managerId;
            model.Abbreviation = input.Abbreviation;
            model.IsDelete = false;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _tenantRepository.InsertAsync(model);
            //清除缓存
            await EventBus.TriggerAsync(new TenantEventData() { });
        }

        /// <summary>
        /// 修改租户信息
        /// </summary>
        /// <param name="input">修改参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">
        /// 抛出异常的情况：
        /// 1. 租户不存在
        /// 2. 租户名称重复
        /// </exception>
        /// <remarks>
        /// 更新内容包括：
        /// 1. 基本信息（名称、合同信息等）
        /// 2. 联系信息（手机、邮箱等）
        /// 3. 状态信息（版本、冻结状态等）
        /// 4. 清除相关缓存
        /// </remarks>
        public async Task UpdateTenant(UpdateTenantInput input)
        {
            var info = await _tenantRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            var isExist = await _tenantRepository.FirstOrDefaultAsync(x => x.Name == input.Name && x.Id != input.Id && x.IsDelete == false);
            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "租户名称重复");
            }
            info.Name = input.Name;
            info.ContractName = input.ContractName;
            info.ContractOwnerName = input.ContractOwnerName;
            info.ContractOwnerMobile = input.ContractOwnerMobile;
            info.ContractStartTime = input.ContractStartTime;
            info.ContractEndTime = input.ContractEndTime;
            info.Mobile = input.Mobile;
            info.StoreLogoUrl = input.StoreLogoUrl;
            info.Remark = input.Remark;
            info.Email = input.Email;
            info.VersionTypeId = input.VersionTypeId;
            info.IsFreeze = input.IsFreeze;
            info.FreezeDes = input.FreezeDes;
            input.Abbreviation = input.Abbreviation;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _tenantRepository.UpdateAsync(info);
            //清除缓存
            await EventBus.TriggerAsync(new TenantEventData() { });
        }

        /// <summary>
        /// 更新租户冻结状态
        /// </summary>
        /// <param name="input">冻结状态更新参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">当租户不存在时抛出异常</exception>
        /// <remarks>
        /// 仅当冻结状态发生变化时才更新：
        /// 1. 更新冻结状态和描述
        /// 2. 记录操作者信息
        /// 3. 清除相关缓存
        /// </remarks>
        public async Task UpdateIsFreezeTenant(UpdateIsFreezeTenantTenantInput input)
        {
            var info = await _tenantRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            if (info.IsFreeze != input.IsFreeze)
            {
                info.IsFreeze = input.IsFreeze;
                info.FreezeDes = input.FreezeDes;
                info.UpdatedAt = DateTime.Now;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                await _tenantRepository.UpdateAsync(info);
            }
            //清除缓存
            await EventBus.TriggerAsync(new TenantEventData() { });
        }

        /// <summary>
        /// 删除租户
        /// </summary>
        /// <param name="input">删除参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">当租户不存在时抛出异常</exception>
        /// <remarks>
        /// 执行软删除：
        /// 1. 更新IsDelete状态
        /// 2. 记录操作者信息
        /// 3. 清除相关缓存
        /// </remarks>
        public async Task DeleteTenant(DeleteTenantInput input)
        {
            var info = await _tenantRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            if (info.IsDelete != input.IsDelete)
            {
                info.IsDelete = input.IsDelete;
                info.UpdatedAt = DateTime.Now;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                await _tenantRepository.UpdateAsync(info);
            }
            //清除缓存
            await EventBus.TriggerAsync(new TenantEventData() { });
        }
    }
}
