using EasyIotSharp.Core.Domain.TenantAccount;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Repositories.TenantAccount;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Services.TenantAccount.Impl
{
    /// <summary>
    /// 用户角色关联服务实现类
    /// </summary>
    public class SoldierRoleService : ServiceBase, ISoldierRoleService
    {
        private readonly ISoldierRepository _soldierRepository;
        private readonly ISoldierRoleRepository _soldierRoleRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="soldierRepository">用户仓储</param>
        /// <param name="soldierRoleRepository">用户角色关联仓储</param>
        public SoldierRoleService(ISoldierRepository soldierRepository,
                                  ISoldierRoleRepository soldierRoleRepository)
        {
            _soldierRepository = soldierRepository;
            _soldierRoleRepository = soldierRoleRepository;
        }

        /// <summary>
        /// 更新用户角色关联
        /// </summary>
        /// <param name="input">更新参数，包含用户ID和角色列表</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">当用户不存在时抛出异常</exception>
        /// <remarks>
        /// 更新流程：
        /// 1. 验证用户是否存在
        /// 2. 删除该用户的所有现有角色关联
        /// 3. 批量创建新的角色关联
        /// 4. 记录操作者信息
        /// </remarks>
        public async Task UpdateSoldierRole(UpdateSoldierRoleInput input)
        {
            var soldier = await _soldierRepository.GetByIdAsync(input.SoldierId);
            if (soldier.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR,"未找到指定的资源");
            }
            //批量删除用户角色表
            await _soldierRoleRepository.DeleteManyBySoldierId(input.SoldierId);
            //批量添加用户角色表
            var soldierRoleInsertList = new List<SoldierRole>();
            foreach (var item in input.Roles)
            {
                soldierRoleInsertList.Add(new SoldierRole()
                {
                    Id = Guid.NewGuid().ToString().Replace("-", ""),
                    TenantNumId = soldier.TenantNumId,
                    SoldierId = soldier.Id,
                    RoleId = item.Id,
                    CreationTime = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    OperatorId = ContextUser.UserId,
                    OperatorName = ContextUser.UserName
                });
            }
            if (soldierRoleInsertList.Count > 0)
            {
                await _soldierRoleRepository.InserManyAsync(soldierRoleInsertList);
            }
        }
    }
}
