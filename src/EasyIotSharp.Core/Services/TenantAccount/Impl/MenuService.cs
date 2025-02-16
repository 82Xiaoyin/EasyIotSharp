﻿using EasyIotSharp.Core.Domain.TenantAccount;
using EasyIotSharp.Core.Dto.TenantAccount;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Repositories.TenantAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.TenantAccount.Impl
{
    public class MenuService : ServiceBase, IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly ISoldierRepository _soldierRepository;
        private readonly IRoleMenuRepository _roleMenuRepository;
        private readonly ISoldierRoleRepository _soldierRoleRepository;

        public MenuService(IMenuRepository menuRepository,
                           IRoleMenuRepository roleMenuRepository,
                           ISoldierRoleRepository soldierRoleRepository,
                           ISoldierRepository soldierRepository)
        {
            _menuRepository = menuRepository;
            _roleMenuRepository = roleMenuRepository;
            _soldierRoleRepository = soldierRoleRepository;
            _soldierRepository = soldierRepository;
        }

        public async Task<MenuDto> GetMenu(string id)
        {
            var info = await _menuRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.Id == id);
            return info.MapTo<MenuDto>();
        }

        public async Task<PagedResultDto<MenuDto>> QueryMenu(QueryMenuInput input)
        {
            var soldier = await _soldierRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.Id == ContextUser.UserId);
            if (soldier.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定资源");
            }
            var query = await _menuRepository.Query(soldier.IsSuperAdmin == true ? 1 : 2, input.Keyword, input.IsEnable, input.PageIndex, input.PageSize);
            int totalCount = query.totalCount;
            var list = query.items.MapTo<List<MenuDto>>();
            return new PagedResultDto<MenuDto>() { TotalCount = totalCount, Items = list };
        }

        public async Task<List<QueryMenuBySoldierIdOutput>> QueryMenuBySoldierId()
        {
            var soldier = await _soldierRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.Id == ContextUser.UserId);
            if (soldier.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR,"未找到指定资源");
            }
            var menus = new List<Menu>();
            if (soldier.IsSuperAdmin==true)
            {
                menus = await _menuRepository.GetAllAsync();
            }
            else
            {
                var soldierRoles = await _soldierRoleRepository.QueryBySoldierId(ContextUser.UserId);
                if (soldierRoles.Count <= 0)
                {
                    return new List<QueryMenuBySoldierIdOutput>();
                }
                var roleIds = soldierRoles.Select(x => x.RoleId).ToList();
                var roleMenus = await _roleMenuRepository.QueryByRoleIds(roleIds);
                if (roleMenus.Count <= 0)
                {
                    return new List<QueryMenuBySoldierIdOutput>();
                }
                var menuIds = roleMenus.Select(x => x.MenuId).ToList();
                menus = await _menuRepository.QueryByIds(menuIds);
            }
            
            if (menus.Count<=0)
            {
                return new List<QueryMenuBySoldierIdOutput>();
            }
            List<QueryMenuBySoldierIdOutput> output = new List<QueryMenuBySoldierIdOutput>();
            var firstMenus = menus.Where(x => string.IsNullOrWhiteSpace(x.ParentId)).ToList();
            foreach (var item in firstMenus)
            {
                QueryMenuBySoldierIdOutput model = new QueryMenuBySoldierIdOutput();
                model.Name = item.Name;
                model.Icon = item.Icon;
                model.Url = item.Url;
                model.Type = item.Type;
                model.Children = new List<ChildrenMenu>();
                model.Children = GetChildrenMenus(menus, item.Id); // 递归获取子菜单
                output.Add(model);
            }
            List<ChildrenMenu> GetChildrenMenus(List<Menu> menus, string parentId)
            {
                var children = new List<ChildrenMenu>();

                // 获取当前菜单的所有子菜单
                var childMenus = menus.Where(x => x.ParentId == parentId).ToList();

                // 遍历子菜单，递归构建子菜单树
                foreach (var item in childMenus)
                {
                    var child = new ChildrenMenu
                    {
                        Name = item.Name,
                        Icon = item.Icon,
                        Url = item.Url,
                        Type = item.Type,
                        Children = GetChildrenMenus(menus, item.Id) // 递归获取子菜单的子菜单
                    };
                    children.Add(child);
                }

                return children;
            }
            return output;
        }

        public async Task InsertMenu(InsertMenuInput input)
        {
            var isExist = await _menuRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.Type == input.Type && (x.Name == input.Name || x.Url == input.Url));
            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "菜单名称或路由重复");
            }
            var model = new Menu();
            model.Id= Guid.NewGuid().ToString().Replace("-", "");
            model.ParentId = input.ParentId;
            model.Name = input.Name;
            model.Icon = input.Icon;
            model.Url = input.Url;
            model.Type = input.Type;
            model.IsEnable = true;
            model.IsSuperAdmin = input.IsSuperAdmin;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _menuRepository.InsertAsync(model);
        }

        public async Task UpdateMenu(UpdateMenuInput input)
        {
            var info = await _menuRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            var isExist = await _menuRepository.FirstOrDefaultAsync(x => x.Id != input.Id && x.IsDelete == false && x.Type == info.Type && (x.Name == input.Name || x.Url == input.Url));
            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "菜单名称或路由重复");
            }
            info.Name = input.Name;
            info.Icon = input.Icon;
            info.Url = input.Url;
            info.IsEnable = input.IsEnable;
            info.IsSuperAdmin = input.IsSuperAdmin;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            info.UpdatedAt = DateTime.Now;
            await _menuRepository.UpdateAsync(info);
        }

        public async Task UpdateIsEnableMenu(UpdateIsEnableMenuInput input)
        {
            var info = await _menuRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            if (info.IsEnable!=input.IsEnable)
            {
                info.IsEnable = info.IsEnable;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                info.UpdatedAt = DateTime.Now;
                await _menuRepository.UpdateAsync(info);
            }
        }

        public async Task DeleteMenu(DeleteMenuInput input)
        {
            var info = await _menuRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            if (info.IsDelete != input.IsDelete)
            {
                info.IsDelete = info.IsDelete;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                info.UpdatedAt = DateTime.Now;
                await _menuRepository.UpdateAsync(info);
            }
        }
    }
}
