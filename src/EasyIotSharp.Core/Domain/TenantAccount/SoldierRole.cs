using SqlSugar;

namespace EasyIotSharp.Core.Domain.TenantAccount
{
    /// <summary>
    /// 用户角色表
    /// </summary>
    [SugarTable("SoldierRoles")]
    public class SoldierRole:BaseEntity<string>
    {
        /// <summary>
        /// 租户id
        /// </summary>
        public int TenantNumId { get; set; }

        /// <summary>
        /// 是否管理员（一个租户只有一个用户拥有一个管理员角色）
        /// 1=管理员
        /// 0=普通用户
        /// </summary>
        public bool IsManager { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public string SoldierId { get; set; }

        /// <summary>
        /// 角色id
        /// </summary>
        public string RoleId { get; set; }
    }
}
