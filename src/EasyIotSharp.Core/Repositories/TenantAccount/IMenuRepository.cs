using EasyIotSharp.Core.Repositories.Mysql;
using EasyIotSharp.Core.Domain.TenantAccount;
using System.Threading.Tasks;
using System.Collections.Generic;
using EasyIotSharp.Core.Dto.TenantAccount;
using EasyIotSharp.Core.Dto.TenantAccount.Params;

namespace EasyIotSharp.Core.Repositories.TenantAccount
{
    public interface IMenuRepository : IMySqlRepositoryBase<Menu, string>
    {
        /// <summary>
        /// 通过条件分页查询菜单列表
        /// </summary>
        /// <param name="isSugerAdmin">-1=不参与查询  1=true  2=false</param>
        /// <param name="keyword"></param>
        /// <param name="isEnable">-1=不参与查询  1=启用  2=禁用</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isPage"></param>
        /// <returns></returns>
        Task<(int totalCount, List<Menu> items)> Query(int isSugerAdmin,string keyword, int isEnable, int pageIndex, int pageSize,bool isPage=true);

        /// <summary>
        /// 通过id集合获取菜单列表
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<List<Menu>> QueryByIds(List<string> ids);
        /// <summary>
        /// 通过父级id创建Tree
        /// </summary>
        /// <param name="menuList"></param>
        /// <returns></returns>
        List<MenuTreeDto> BuildMenuTree(List<MenuDto> menuList);

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<bool> VerifyMenu(InsertMenuInput input);
    }
}
