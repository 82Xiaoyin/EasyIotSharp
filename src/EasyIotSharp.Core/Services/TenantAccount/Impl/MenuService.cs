using EasyIotSharp.Core.Domain.TenantAccount;
using EasyIotSharp.Core.Dto.TenantAccount;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Repositories.TenantAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.TenantAccount.Impl
{
    public class MenuService : ServiceBase, IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly ISoldierRepository _soldierRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleMenuRepository _roleMenuRepository;
        private readonly ISoldierRoleRepository _soldierRoleRepository;

        public MenuService(IMenuRepository menuRepository,
                           IRoleMenuRepository roleMenuRepository,
                           ISoldierRoleRepository soldierRoleRepository,
                           ISoldierRepository soldierRepository,
                           IRoleRepository roleRepository)
        {
            _menuRepository = menuRepository;
            _roleMenuRepository = roleMenuRepository;
            _soldierRoleRepository = soldierRoleRepository;
            _soldierRepository = soldierRepository;
            _roleRepository = roleRepository;
        }

        public async Task<MenuDto> GetMenu(string id)
        {
            var info = await _menuRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.Id == id);
            return info.MapTo<MenuDto>();
        }

        public async Task<PagedResultDto<MenuTreeDto>> QueryMenu(QueryMenuInput input)
        {
            var soldier = await _soldierRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.Id == ContextUser.UserId);
            if (soldier.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定资源");
            }
            // 查询菜单数据
            var query = await _menuRepository.Query(soldier.IsSuperAdmin == true ? -1 : 0, input.Keyword, input.IsEnable, input.PageIndex, input.PageSize, false);
            int totalCount = query.totalCount;
            var list = query.items.MapTo<List<MenuDto>>();

            // 构建树形结构
            var tree = _menuRepository.BuildMenuTree(list);
            return new PagedResultDto<MenuTreeDto>() { TotalCount = totalCount, Items = tree };
        }

        public async Task<(List<QueryMenuBySoldierIdOutput> output, List<Menu> menus)> QueryMenuBySoldierId(bool isTreeResult = true)
        {
            var soldier = await _soldierRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.Id == ContextUser.UserId);
            if (soldier.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定资源");
            }
            var menus = new List<Menu>();
            if (soldier.IsSuperAdmin == true)
            {
                menus = await _menuRepository.GetAllAsync();
            }
            else
            {
                var soldierRoles = await _soldierRoleRepository.QueryBySoldierId(ContextUser.UserId);
                if (soldierRoles.Count <= 0)
                {
                    return (new List<QueryMenuBySoldierIdOutput>(), new List<Menu>());
                }
                var soldierRoleIds = soldierRoles.Select(x => x.RoleId).ToList();
                var roles = await _roleRepository.QueryByIds(soldierRoleIds);
                if (roles.Count <= 0)
                {
                    return (new List<QueryMenuBySoldierIdOutput>(), new List<Menu>());
                }
                var roleIds = roles.Select(x => x.Id).ToList();
                var roleMenus = await _roleMenuRepository.QueryByRoleIds(roleIds);
                if (roleMenus.Count <= 0)
                {
                    return (new List<QueryMenuBySoldierIdOutput>(), new List<Menu>());
                }
                var menuIds = roleMenus.Select(x => x.MenuId).ToList();
                menus = await _menuRepository.QueryByIds(menuIds);
            }

            if (menus.Count <= 0)
            {
                return (new List<QueryMenuBySoldierIdOutput>(), new List<Menu>());
            }
            if (isTreeResult == false)
            {
                return (new List<QueryMenuBySoldierIdOutput>(), menus);
            }
            List<QueryMenuBySoldierIdOutput> output = new List<QueryMenuBySoldierIdOutput>();
            var firstMenus = menus.Where(x => string.IsNullOrWhiteSpace(x.ParentId)).OrderBy(x => x.CreationTime).ToList();
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
                var childMenus = menus.Where(x => x.ParentId == parentId).OrderBy(x => x.CreationTime).ToList();

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
            output = output.OrderByDescending(x => x.Type).ToList();
            return (output, menus);
        }

        public async Task<List<string>> QueryUrlMenuByParentUrl(QueryUrlMenuByParentUrlInput input)
        {
            var menus = await QueryMenuBySoldierId(false);
            var parentMenu = menus.menus.FirstOrDefault(x => x.Url == input.Url);
            if (parentMenu.IsNull())
            {
                return new List<string>();
            }
            var children = menus.menus.Where(x => x.Type == 3 && x.IsDelete == false && x.IsEnable == true && x.ParentId == parentMenu.Id);
            return children.Select(x => x.Url).ToList();
        }

        public async Task InsertMenu(InsertMenuInput input)
        {
            var isExist = await _menuRepository.VerifyMenu(input);
            if (isExist)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "菜单名称或路由重复");
            }
            var model = new Menu();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.ParentId = input.ParentId;
            model.Name = input.Name;
            model.Icon = input.Icon;
            model.Url = input.Url;
            model.Type = input.Type;
            model.Sort = input.Sort;
            model.IsEnable = true;
            model.IsSuperAdmin = input.IsSuperAdmin;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _menuRepository.InsertAsync(model);
        }

        public async Task UpdateMenu(InsertMenuInput input)
        {
            var info = await _menuRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            var isExist = await _menuRepository.VerifyMenu(input);
            if (isExist)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "菜单名称或路由重复");
            }
            info.Name = input.Name;
            info.Icon = input.Icon;
            info.Url = input.Url;
            info.Sort = input.Sort;
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
            if (info.IsEnable != input.IsEnable)
            {
                info.IsEnable = input.IsEnable;
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
                info.IsDelete = input.IsDelete;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                info.UpdatedAt = DateTime.Now;
                await _menuRepository.UpdateAsync(info);
            }
        }
    }
}
