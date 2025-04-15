using EasyIotSharp.Core.Domain.TenantAccount;
using EasyIotSharp.Core.Dto.TenantAccount;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.TenantAccount
{
    public interface IMenuService
    {
        /// <summary>
        /// 通过id获取一条菜单信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MenuDto> GetMenu(string id);

        /// <summary>
        /// 通过id获取一条菜单信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<MenuTreeDto>> QueryMenu(QueryMenuInput input);

        /// <summary>
        /// 通过用户id获取用户对应的菜单列表
        /// </summary>
        /// <returns></returns>
        Task<(List<QueryMenuBySoldierIdOutput> output, List<Menu> menus)> QueryMenuBySoldierId(bool isTreeResult = true);

        /// <summary>
        /// 通过父级url获取按钮级别的url
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<List<string>> QueryUrlMenuByParentUrl(QueryUrlMenuByParentUrlInput input);

        /// <summary>
        /// 添加一条菜单信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task InsertMenu(InsertMenuInput input);

        /// <summary>
        /// 通过id修改一条菜单信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateMenu(InsertMenuInput input);

        /// <summary>
        /// 通过id修改一条菜单是否启用状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateIsEnableMenu(UpdateIsEnableMenuInput input);
        /// <summary>
        /// 通过id获取一条菜单信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteMenu(DeleteMenuInput input);
    }
}
