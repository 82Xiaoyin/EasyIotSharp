using EasyIotSharp.Core.Caching.TenantAccount;
using EasyIotSharp.Core.Domain.TenantAccount;
using EasyIotSharp.Core.Dto.TenantAccount;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Events.Tenant;
using EasyIotSharp.Core.Events.TenantAccount;
using EasyIotSharp.Core.Repositories.TenantAccount;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.TenantAccount.Impl
{
    /// <summary>
    /// 菜单服务实现类
    /// </summary>
    public class MenuService : ServiceBase, IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly ISoldierRepository _soldierRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleMenuRepository _roleMenuRepository;
        private readonly ISoldierRoleRepository _soldierRoleRepository;
        private readonly IMenuCacheService _menuCacheService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="menuRepository">菜单仓储</param>
        /// <param name="roleMenuRepository">角色菜单仓储</param>
        /// <param name="soldierRoleRepository">用户角色仓储</param>
        /// <param name="soldierRepository">用户仓储</param>
        /// <param name="roleRepository">角色仓储</param>
        /// <param name="menuCacheService">菜单缓存服务</param>
        public MenuService(IMenuRepository menuRepository,
                           IRoleMenuRepository roleMenuRepository,
                           ISoldierRoleRepository soldierRoleRepository,
                           ISoldierRepository soldierRepository,
                           IRoleRepository roleRepository,
                           IMenuCacheService menuCacheService)
        {
            _menuRepository = menuRepository;
            _roleMenuRepository = roleMenuRepository;
            _soldierRoleRepository = soldierRoleRepository;
            _soldierRepository = soldierRepository;
            _roleRepository = roleRepository;
            _menuCacheService = menuCacheService;
        }

        /// <summary>
        /// 获取单个菜单信息
        /// </summary>
        /// <param name="id">菜单ID</param>
        /// <returns>菜单信息</returns>
        public async Task<MenuDto> GetMenu(string id)
        {
            var info = await _menuRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.Id == id);
            return info.MapTo<MenuDto>();
        }

        /// <summary>
        /// 查询菜单列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页后的菜单树形结构</returns>
        /// <remarks>
        /// 当满足以下条件时使用缓存：
        /// 1. 无关键字搜索
        /// 2. 启用状态为-1
        /// 3. 使用分页且在前5页
        /// 4. 每页大小为10
        /// </remarks>
        public async Task<PagedResultDto<MenuTreeDto>> QueryMenu(QueryMenuInput input)
        {
            if (string.IsNullOrEmpty(input.Keyword) 
                && input.IsEnable.Equals(-1)
                && input.IsPage.Equals(true)
                && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _menuCacheService.QueryMenu(input, async () =>
                {
                    var soldier = await _soldierRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.Id == ContextUser.UserId);
                    if (soldier.IsNull())
                    {
                        throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定资源");
                    }
                    // 查询菜单数据
                    var query = await _menuRepository.Query(soldier.IsSuperAdmin == true ? -1 : 0, input.Keyword, input.IsEnable, input.PageIndex, input.PageSize, false);
                    var list = query.items.MapTo<List<MenuDto>>();
                    // 构建树形结构
                    var tree = _menuRepository.BuildMenuTree(list).Skip((input.PageIndex - 1) * input.PageSize)
                                      .Take(input.PageSize).ToList();
                    int totalCount = tree.Count;
                    return new PagedResultDto<MenuTreeDto>() { TotalCount = totalCount, Items = tree };
                });
            }
            else
            {
                var soldier = await _soldierRepository.FirstOrDefaultAsync(x => x.IsDelete == false && x.Id == ContextUser.UserId);
                if (soldier.IsNull())
                {
                    throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定资源");
                }
                // 查询菜单数据
                var query = await _menuRepository.Query(soldier.IsSuperAdmin == true ? -1 : 0, input.Keyword, input.IsEnable, input.PageIndex, input.PageSize, false);
                var list = query.items.MapTo<List<MenuDto>>();

                // 构建树形结构
                var tree = _menuRepository.BuildMenuTree(list).Skip((input.PageIndex - 1) * input.PageSize)
                                  .Take(input.PageSize).ToList();
                int totalCount = tree.Count;
                return new PagedResultDto<MenuTreeDto>() { TotalCount = totalCount, Items = tree };
            }
        }

        /// <summary>
        /// 根据用户ID查询菜单
        /// </summary>
        /// <param name="isTreeResult">是否返回树形结构</param>
        /// <returns>菜单列表和原始菜单数据</returns>
        /// <remarks>
        /// 处理逻辑：
        /// 1. 超级管理员获取所有菜单
        /// 2. 普通用户通过角色关联获取菜单
        /// 3. 可选择返回树形或平铺结构
        /// </remarks>
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

        /// <summary>
        /// 根据父级URL查询子菜单URL列表
        /// </summary>
        /// <param name="input">父级URL参数</param>
        /// <returns>子菜单URL列表</returns>
        /// <remarks>
        /// 仅返回：
        /// 1. Type为3的菜单
        /// 2. 未删除的菜单
        /// 3. 已启用的菜单
        /// </remarks>
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

        /// <summary>
        /// 新增菜单
        /// </summary>
        /// <param name="input">新增参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">当菜单名称或路由重复时抛出异常</exception>
        /// <remarks>
        /// 创建内容包括：
        /// 1. 基本信息（名称、图标、URL等）
        /// 2. 层级关系（父级ID）
        /// 3. 权限信息（超级管理员权限）
        /// 4. 清除相关缓存
        /// </remarks>
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

            //清除缓存
            await EventBus.TriggerAsync(new MenuEventData() { });
        }

        /// <summary>
        /// 修改菜单信息
        /// </summary>
        /// <param name="input">修改参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">
        /// 抛出异常的情况：
        /// 1. 菜单不存在
        /// 2. 菜单名称或路由重复
        /// </exception>
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

            //清除缓存
            await EventBus.TriggerAsync(new MenuEventData() { });
        }

        /// <summary>
        /// 更新菜单启用状态
        /// </summary>
        /// <param name="input">启用状态更新参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">当菜单不存在时抛出异常</exception>
        /// <remarks>
        /// 仅当启用状态发生变化时才更新：
        /// 1. 更新启用状态
        /// 2. 记录操作者信息
        /// 3. 清除相关缓存
        /// </remarks>
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

            //清除缓存
            await EventBus.TriggerAsync(new MenuEventData() { });
        }

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="input">删除参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">当菜单不存在时抛出异常</exception>
        /// <remarks>
        /// 执行软删除：
        /// 1. 更新IsDelete状态
        /// 2. 记录操作者信息
        /// 3. 清除相关缓存
        /// </remarks>
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

            //清除缓存
            await EventBus.TriggerAsync(new MenuEventData() { });
        }
    }
}
