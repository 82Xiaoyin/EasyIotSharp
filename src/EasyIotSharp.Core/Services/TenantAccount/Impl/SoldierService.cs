using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Dto.TenantAccount;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Repositories.TenantAccount;
using UPrime.AutoMapper;
using EasyIotSharp.Core.Repositories.Tenant;
using EasyIotSharp.Core.Domain.TenantAccount;
using EasyIotSharp.Core.Extensions;
using static EasyIotSharp.Core.GlobalConsts;
using EasyIotSharp.Core.Dto.Tenant;
using System.Linq;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using EasyIotSharp.Core.Caching.TenantAccount;
using EasyIotSharp.Core.Caching.TenantAccount.Impl;
using EasyIotSharp.Core.Events.TenantAccount;

namespace EasyIotSharp.Core.Services.TenantAccount.Impl
{
    /// <summary>
    /// 用户服务实现类
    /// </summary>
    public class SoldierService : ServiceBase, ISoldierService
    {
        private readonly ISoldierRepository _soldierRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ISoldierRoleRepository _soldierRoleRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ISoldierCacheService _soldierCacheService;
        private readonly IRoleService _roleService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="soldierRepository">用户仓储</param>
        /// <param name="tenantRepository">租户仓储</param>
        /// <param name="soldierRoleRepository">用户角色关联仓储</param>
        /// <param name="roleService">角色服务</param>
        /// <param name="roleRepository">角色仓储</param>
        /// <param name="soldierCacheService">用户缓存服务</param>
        public SoldierService(ISoldierRepository soldierRepository,
                              ITenantRepository tenantRepository,
                              ISoldierRoleRepository soldierRoleRepository,
                              IRoleService roleService,
                              IRoleRepository roleRepository,
                              ISoldierCacheService soldierCacheService)
        {
            _soldierRepository = soldierRepository;
            _tenantRepository = tenantRepository;
            _soldierRoleRepository = soldierRoleRepository;
            _roleRepository = roleRepository;

            _roleService = roleService;
            _soldierCacheService = soldierCacheService;
        }

        /// <summary>
        /// 获取单个用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户信息</returns>
        public async Task<SoldierDto> GetSoldier(string id)
        {
            var info = await _soldierRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.Id == id);
            return info.MapTo<SoldierDto>();
        }

        /// <summary>
        /// 验证用户登录
        /// </summary>
        /// <param name="input">登录参数</param>
        /// <returns>验证结果，包含用户信息、租户信息和Token</returns>
        /// <remarks>
        /// 验证流程：
        /// 1. 验证用户名密码
        /// 2. 检查用户状态
        /// 3. 验证租户状态（存在性、删除状态、冻结状态、合同有效期）
        /// 4. 更新最后登录时间
        /// 5. 生成访问令牌
        /// </remarks>
        public async Task<ValidateSoldierOutput> ValidateSoldier(ValidateSoldierInput input)
        {
            ValidateSoldierOutput res = new ValidateSoldierOutput();
            input.Mobile = input.Mobile.Trim();
            input.Password = input.Password.Trim();

            input.Password = AESHelper.Decrypt(input.Password, AES_KEY.Key);
            var md5Password = input.Password.Md5();

            //有同时存在不同的机构，不同职位同一个手机号的情况
            var user = await _soldierRepository.FirstOrDefaultAsync(x => x.Mobile == input.Mobile && x.Password == md5Password && x.IsDelete == false);
            if (user.IsNull())
            {
                res.Status = ValidateSoldierStatus.InvalidNameOrPassword;
                return res;
            }
            if (user.IsEnable == false)
            {
                res.Status = ValidateSoldierStatus.SoldiersIsDisable;
                return res;
            }
            var tenant = await _tenantRepository.FirstOrDefaultAsync(x => x.NumId == user.TenantNumId);
            if (tenant.IsNull())
            {
                res.Status = ValidateSoldierStatus.TenantIsNotExists;
            }
            if (tenant.IsDelete)
            {
                res.Status = ValidateSoldierStatus.TenantIsDeleted;
                return res;
            }
            if (tenant.IsFreeze)
            {
                res.Status = ValidateSoldierStatus.TenantIsFreeze;
                return res;
            }
            if (tenant.ContractEndTime < DateTime.Now)
            {
                res.Status = ValidateSoldierStatus.TenantIsExpired;
                return res;
            }
            user.LastLoginTime = DateTime.Now;
            //更新登录时间
            await _soldierRepository.UpdateAsync(user);
            res.Solider = user.MapTo<SoldierDto>();
            res.Tenant = tenant.MapTo<TenantDto>();
            res.Token = TokenExtensions.GenerateToken(res.Solider.Id, res.Solider.Username, res.Tenant.Id, res.Tenant.NumId.ToString(),res.Tenant.Abbreviation);
            return res;
        }

        /// <summary>
        /// 查询用户列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页后的用户列表</returns>
        /// <remarks>
        /// 当满足以下条件时使用缓存：
        /// 1. 无关键字搜索
        /// 2. 启用状态为-1
        /// 3. 使用分页且在前5页
        /// 4. 每页大小为10
        /// 
        /// 查询结果包含：
        /// 1. 基本用户信息
        /// 2. 关联的角色信息
        /// </remarks>
        public async Task<PagedResultDto<SoldierDto>> QuerySoldier(QuerySoldierInput input)
        {

            if (string.IsNullOrEmpty(input.Keyword)
                                && input.IsEnable.Equals(-1)
                                && input.IsPage.Equals(true)
                                && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _soldierCacheService.QuerySoldier(input, async () =>
                {
                    var query = await _soldierRepository.Query(ContextUser.TenantNumId, input.Keyword, input.IsEnable, input.PageIndex, input.PageSize);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<SoldierDto>>();
                    var soldierIds = list.Select(x => x.Id).ToList();
                    if (soldierIds.Count > 0)
                    {
                        var soldierRoles = await _soldierRoleRepository.QueryBySoldierIds(soldierIds);
                        var roles = await _roleRepository.QueryByIds(soldierRoles.Select(x => x.RoleId).ToList());
                        foreach (var item in list)
                        {
                            var soldierRole = soldierRoles.FirstOrDefault(x => x.SoldierId == item.Id);
                            if (soldierRole.IsNotNull())
                            {
                                item.RoleId = roles.FirstOrDefault(x => x.Id == soldierRole.RoleId)?.Id;
                            }
                        }
                    }
                    return new PagedResultDto<SoldierDto>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _soldierRepository.Query(ContextUser.TenantNumId, input.Keyword, input.IsEnable, input.PageIndex, input.PageSize);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<SoldierDto>>();
                var soldierIds = list.Select(x => x.Id).ToList();
                if (soldierIds.Count > 0)
                {
                    var soldierRoles = await _soldierRoleRepository.QueryBySoldierIds(soldierIds);
                    var roles = await _roleRepository.QueryByIds(soldierRoles.Select(x => x.RoleId).ToList());
                    foreach (var item in list)
                    {
                        var soldierRole = soldierRoles.FirstOrDefault(x => x.SoldierId == item.Id);
                        if (soldierRole.IsNotNull())
                        {
                            item.RoleId = roles.FirstOrDefault(x => x.Id == soldierRole.RoleId)?.Id;
                        }
                    }
                }
                return new PagedResultDto<SoldierDto>() { TotalCount = totalCount, Items = list };
            }
        }

        /// <summary>
        /// 新增普通用户
        /// </summary>
        /// <param name="input">新增参数</param>
        /// <returns>新创建的用户ID</returns>
        /// <exception cref="BizException">
        /// 抛出异常的情况：
        /// 1. 租户不存在或已冻结
        /// 2. 手机号已存在
        /// 3. 用户名重复
        /// 4. 管理员角色重复分配
        /// </exception>
        /// <remarks>
        /// 创建内容包括：
        /// 1. 基本用户信息（默认密码123456）
        /// 2. 用户角色关联
        /// 3. 清除相关缓存
        /// </remarks>
        public async Task<string> InsertSoldier(InsertSoldierInput input)
        {
            var tenant = await _tenantRepository.FirstOrDefaultAsync(x => x.NumId == ContextUser.TenantNumId && x.IsDelete == false);
            if (tenant.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "租户信息不存在");
            }
            if (tenant.IsFreeze == true)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "租户已冻结，请联系管理员");
            }
            var isExistMobile = await _soldierRepository.FirstOrDefaultAsync(x => x.Mobile == input.Mobile && x.IsDelete == false);
            if (isExistMobile.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "手机号已存在");
            }
            var isExistUsername = await _soldierRepository.FirstOrDefaultAsync(x => x.Username == input.Username && x.TenantNumId == ContextUser.TenantNumId && x.IsDelete == false);
            if (isExistUsername.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "用户名重复");
            }
            var isExistManager = await _soldierRoleRepository.FirstOrDefaultAsync(x => x.IsManager == 2 && x.RoleId == input.RoleId && x.TenantNumId == ContextUser.TenantNumId && x.IsDelete == false);
            if (isExistManager.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "只能有一个用户分配管理员角色");
            }
            var model = new Soldier();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.TenantNumId = ContextUser.TenantNumId;
            model.IsSuperAdmin = false;
            model.IsManager = 2;
            model.Mobile = input.Mobile;
            model.Username = input.Username;
            model.Password = "123456".Md5();
            model.IsTest = false;
            model.Sex = input.Sex;
            model.IsEnable = true;
            model.Email = input.Email;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _soldierRepository.InsertAsync(model);

            //添加一个用户角色信息
            await _soldierRoleRepository.InsertAsync(new SoldierRole()
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                TenantNumId = ContextUser.TenantNumId,
                IsManager = model.IsManager,
                SoldierId = model.Id,
                RoleId = input.RoleId,
                OperatorId = ContextUser.UserId,
                OperatorName = ContextUser.UserName,
            });

            //清除缓存
            await EventBus.TriggerAsync(new SoldierEventData() { });

            return model.Id;
        }

        /// <summary>
        /// 新增管理员用户
        /// </summary>
        /// <param name="input">新增参数</param>
        /// <returns>新创建的用户ID</returns>
        /// <exception cref="BizException">当手机号已存在时抛出异常</exception>
        /// <remarks>
        /// 创建内容包括：
        /// 1. 管理员用户信息（IsManager=1）
        /// 2. 创建管理员角色
        /// 3. 用户角色关联
        /// 4. 清除相关缓存
        /// </remarks>
        public async Task<string> InsertAdminSoldier(InsertAdminSoldierInput input)
        {
            var isExistMobile = await _soldierRepository.FirstOrDefaultAsync(x => x.Mobile == input.Mobile && x.IsDelete == false);
            if (isExistMobile.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "手机号已存在");
            }
            var model = new Soldier();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.TenantNumId = ContextUser.TenantNumId;
            model.IsSuperAdmin = false;
            model.IsManager = 1;
            model.Mobile = input.Mobile;
            model.Username = input.Username;
            model.Password = "123456".Md5();
            model.IsTest = false;
            model.Sex = input.Sex;
            model.IsEnable = true;
            model.Email = input.Email;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _soldierRepository.InsertAsync(model);

            //创建一个管理员角色，包含所有非系统级别启用的菜单
            var roleId = await _roleService.InsertAdminRole(new InsertAdminRoleInput()
            {
                Name = "管理员",
                Remark = ""
            });
            //添加一个用户角色信息
            await _soldierRoleRepository.InsertAsync(new SoldierRole()
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                TenantNumId = ContextUser.TenantNumId,
                IsManager = model.IsManager,
                SoldierId = model.Id,
                RoleId = roleId,
                OperatorId = ContextUser.UserId,
                OperatorName = ContextUser.UserName,
            });

            //清除缓存
            await EventBus.TriggerAsync(new SoldierEventData() { });
            return model.Id;
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="input">更新参数</param>
        /// <returns>更新的用户ID</returns>
        /// <exception cref="BizException">
        /// 抛出异常的情况：
        /// 1. 用户不存在
        /// 2. 用户名重复
        /// 3. 管理员角色重复分配
        /// </exception>
        /// <remarks>
        /// 更新内容包括：
        /// 1. 基本信息（用户名、性别等）
        /// 2. 可选更新角色信息
        /// 3. 清除相关缓存
        /// </remarks>
        public async Task<string> UpdateSoldier(UpdateSoldierInput input)
        {
            var info = await _soldierRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "用户信息不存在");
            }
            var isExistUsername = await _soldierRepository.FirstOrDefaultAsync(x => x.Username == input.Username && x.TenantNumId == info.TenantNumId && x.Id != input.Id && x.IsDelete == false);
            if (isExistUsername.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "用户名重复");
            }
            info.Username = input.Username;
            info.Sex = input.Sex;
            info.IsEnable = input.IsEnable;
            info.Email = input.Email;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            info.UpdatedAt = DateTime.Now;
            await _soldierRepository.UpdateAsync(info);

            if (input.IsUpdateRole == true)
            {
                var isExistManager = await _soldierRoleRepository.FirstOrDefaultAsync(x => x.IsManager == 2 && x.RoleId == input.RoleId && x.TenantNumId == ContextUser.TenantNumId && x.IsDelete == false);
                if (isExistManager.IsNotNull() && input.Id != isExistManager.Id)
                {
                    throw new BizException(BizError.BIND_EXCEPTION_ERROR, "只能有一个用户分配管理员角色");
                }
                //删除一条用户角色表
                await _soldierRoleRepository.DeleteManyBySoldierId(info.Id);

                //添加一个用户角色信息
                await _soldierRoleRepository.InsertAsync(new SoldierRole()
                {
                    Id = Guid.NewGuid().ToString().Replace("-", ""),
                    TenantNumId = ContextUser.TenantNumId,
                    IsManager = info.IsManager,
                    SoldierId = info.Id,
                    RoleId = input.RoleId,
                    OperatorId = ContextUser.UserId,
                    OperatorName = ContextUser.UserName,
                });
            }

            //清除缓存
            await EventBus.TriggerAsync(new SoldierEventData() { });

            return info.Id;
        }

        /// <summary>
        /// 更新用户启用状态
        /// </summary>
        /// <param name="input">启用状态更新参数</param>
        /// <returns>更新的用户ID</returns>
        /// <exception cref="BizException">当用户不存在时抛出异常</exception>
        /// <remarks>
        /// 仅当启用状态发生变化时才更新：
        /// 1. 更新启用状态
        /// 2. 记录操作者信息
        /// 3. 清除相关缓存
        /// </remarks>
        public async Task<string> UpdateSoldierIsEnable(UpdateSoldierIsEnableInput input)
        {
            var info = await _soldierRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "用户信息不存在");
            }
            if (info.IsEnable != input.IsEnable)
            {
                info.IsEnable = input.IsEnable;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                info.UpdatedAt = DateTime.Now;
                await _soldierRepository.UpdateAsync(info);
            }

            //清除缓存
            await EventBus.TriggerAsync(new SoldierEventData() { });

            return info.Id;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="input">删除参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">当用户不存在时抛出异常</exception>
        /// <remarks>
        /// 执行软删除：
        /// 1. 更新IsDelete状态
        /// 2. 记录操作者信息
        /// 3. 清除相关缓存
        /// </remarks>
        public async Task DeleteSoldier(DeleteSoldierInput input)
        {
            var info = await _soldierRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "用户信息不存在");
            }
            if (info.IsDelete != input.IsDelete)
            {
                info.IsDelete = input.IsDelete;
                info.UpdatedAt = DateTime.Now;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                await _soldierRepository.UpdateAsync(info);
            }

            //清除缓存
            await EventBus.TriggerAsync(new SoldierEventData() { });
        }
    }
}
