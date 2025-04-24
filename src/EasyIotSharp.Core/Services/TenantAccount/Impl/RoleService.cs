using EasyIotSharp.Core.Dto.TenantAccount;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Repositories.TenantAccount;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.AutoMapper;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Domain.TenantAccount;
using EasyIotSharp.Core.Caching.TenantAccount;
using EasyIotSharp.Core.Events.TenantAccount;

namespace EasyIotSharp.Core.Services.TenantAccount.Impl
{
    public class RoleService : ServiceBase, IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleMenuRepository _roleMenuRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly IRoleCacheService _roleCacheService;

        public RoleService(IRoleRepository roleRepository,
                           IRoleMenuRepository roleMenuRepository,
                           IMenuRepository menuRepository,
                           IRoleCacheService roleCacheService)
        {
            _roleRepository = roleRepository;
            _roleMenuRepository = roleMenuRepository;
            _menuRepository = menuRepository;
            _roleCacheService = roleCacheService;
        }

        public async Task<GetRoleMenuOutput> GetRoleMenu(string id)
        {
            var role = await _roleRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            if (role.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未请求到指定的资源");
            }
            GetRoleMenuOutput output = new GetRoleMenuOutput();
            output = role.MapTo<GetRoleMenuOutput>();
            output.Menus = new List<string>();
            var roleMenus = await _roleMenuRepository.GetListAsync(x => x.RoleId == id);
            if (roleMenus.Count > 0)
            {
                var menus = roleMenus.Select(x => x.MenuId).ToList();
                output.Menus = menus.MapTo<List<string>>();
            }
            return output;
        }

        public async Task<PagedResultDto<RoleDto>> QueryRole(QueryRoleInput input)
        {
            if (string.IsNullOrEmpty(input.Keyword)
                                && input.IsEnable.Equals(-1)
                                && input.IsPage.Equals(true)
                                && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _roleCacheService.QueryRole(input, async () =>
                {
                    var query = await _roleRepository.Query(ContextUser.TenantNumId, input.Keyword, input.IsEnable, input.PageIndex, input.PageSize, input.IsPage);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<RoleDto>>();
                    return new PagedResultDto<RoleDto>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _roleRepository.Query(ContextUser.TenantNumId, input.Keyword, input.IsEnable, input.PageIndex, input.PageSize, input.IsPage);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<RoleDto>>();
                return new PagedResultDto<RoleDto>() { TotalCount = totalCount, Items = list };
            }
        }

        public async Task InsertRole(InsertRoleInput input)
        {
            var isExist = await _roleRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.TenantNumId == ContextUser.TenantNumId && x.Name == input.Name);
            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "名字重复");
            }
            var roleMedel = new Role();
            roleMedel.Id = Guid.NewGuid().ToString().Replace("-", "");
            roleMedel.TenantNumId = ContextUser.TenantNumId;
            roleMedel.IsManager = 2;
            roleMedel.Name = input.Name;
            roleMedel.Remark = input.Remark;
            roleMedel.IsEnable = input.IsEnable;
            roleMedel.CreationTime = DateTime.Now;
            roleMedel.UpdatedAt = DateTime.Now;
            roleMedel.OperatorId = ContextUser.UserId;
            roleMedel.OperatorName = ContextUser.UserName;
            await _roleRepository.InsertAsync(roleMedel);
            var roleMenuInsertList = new List<RoleMenu>();
            foreach (var item in input.Menus)
            {
                roleMenuInsertList.Add(new RoleMenu()
                {
                    Id = Guid.NewGuid().ToString().Replace("-", ""),
                    TenantNumId = ContextUser.TenantNumId,
                    RoleId = roleMedel.Id,
                    MenuId = item,
                    CreationTime = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    OperatorId = ContextUser.UserId,
                    OperatorName = ContextUser.UserName
                });
            }
            if (roleMenuInsertList.Count > 0)
            {
                await _roleMenuRepository.InserManyAsync(roleMenuInsertList);
            }

            //清除缓存
            await EventBus.TriggerAsync(new RoleEventData() { });
        }

        public async Task<string> InsertAdminRole(InsertAdminRoleInput input)
        {

            var roleMedel = new Role();
            roleMedel.Id = Guid.NewGuid().ToString().Replace("-", "");
            roleMedel.TenantNumId = ContextUser.TenantNumId;
            roleMedel.IsManager = 1;
            roleMedel.Name = input.Name;
            roleMedel.Remark = input.Remark;
            roleMedel.IsEnable = true;
            roleMedel.CreationTime = DateTime.Now;
            roleMedel.UpdatedAt = DateTime.Now;
            roleMedel.OperatorId = ContextUser.UserId;
            roleMedel.OperatorName = ContextUser.UserName;
            await _roleRepository.InsertAsync(roleMedel);
            //创建一个管理员角色，包含所有非系统级别启用的菜单
            var menus = await _menuRepository.Query(0, "", 1, 0, 0, false);
            var roleMenuInsertList = new List<RoleMenu>();
            foreach (var item in menus.items)
            {
                roleMenuInsertList.Add(new RoleMenu()
                {
                    Id = Guid.NewGuid().ToString().Replace("-", ""),
                    TenantNumId = ContextUser.TenantNumId,
                    RoleId = roleMedel.Id,
                    MenuId = item.Id,
                    CreationTime = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    OperatorId = ContextUser.UserId,
                    OperatorName = ContextUser.UserName
                });
            }
            if (roleMenuInsertList.Count > 0)
            {
                await _roleMenuRepository.InserManyAsync(roleMenuInsertList);
            }
            return roleMedel.Id;
        }

        public async Task UpdateRole(UpdateRoleInput input)
        {
            var role = await _roleRepository.GetByIdAsync(input.Id);
            if (role.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定的资源");
            }
            var isExist = await _roleRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.Id != input.Id && x.TenantNumId == ContextUser.TenantNumId && x.Name == input.Name);
            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "名字重复");
            }
            role.Name = input.Name;
            role.Remark = input.Remark;
            role.IsEnable = input.IsEnable;
            role.UpdatedAt = DateTime.Now;
            role.OperatorId = ContextUser.UserId;
            role.OperatorName = ContextUser.UserName;
            await _roleRepository.UpdateAsync(role);
            if (input.IsUpdateMenu == true)
            {
                //批量删除老的角色菜单表
                await _roleMenuRepository.DeleteManyByRoleId(role.Id);

                //批量添加新的角色菜单表
                var roleMenuInsertList = new List<RoleMenu>();
                foreach (var item in input.Menus)
                {
                    roleMenuInsertList.Add(new RoleMenu()
                    {
                        Id = Guid.NewGuid().ToString().Replace("-", ""),
                        TenantNumId = ContextUser.TenantNumId,
                        RoleId = role.Id,
                        MenuId = item,
                        CreationTime = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        OperatorId = ContextUser.UserId,
                        OperatorName = ContextUser.UserName
                    });
                }
                if (roleMenuInsertList.Count > 0)
                {
                    await _roleMenuRepository.InserManyAsync(roleMenuInsertList);
                }
            }

            //清除缓存
            await EventBus.TriggerAsync(new RoleEventData() { });
        }

        public async Task UpdateIsEnableRole(UpdateIsEnableRoleInput input)
        {
            var info = await _roleRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            if (info.IsManager == 1)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "管理员角色不可禁用");
            }
            if (info.IsEnable != input.IsEnable)
            {
                info.IsEnable = input.IsEnable;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                info.UpdatedAt = DateTime.Now;
                await _roleRepository.UpdateAsync(info);
            }
        }

        public async Task DeleteRole(DeleteRoleInput input)
        {
            var info = await _roleRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            if (info.IsManager == 1)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "管理员角色不可删除");
            }
            if (info.IsDelete != input.IsDelete)
            {
                info.IsDelete = input.IsDelete;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                info.UpdatedAt = DateTime.Now;
                await _roleRepository.UpdateAsync(info);
            }

            //清除缓存
            await EventBus.TriggerAsync(new RoleEventData() { });
        }
    }
}
