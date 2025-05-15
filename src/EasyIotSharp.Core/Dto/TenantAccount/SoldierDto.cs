using EasyIotSharp.Core.Domain;
using EasyIotSharp.Core.Domain.TenantAccount;
using System;
using UPrime.AutoMapper;

namespace EasyIotSharp.Core.Dto.TenantAccount
{
    [AutoMapFrom(typeof(Soldier))]
    public class SoldierDto
    {
        public string Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime? DeleteTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 操作人标识
        /// </summary>
        public string OperatorId { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string OperatorName { get; set; }

        /// <summary>
        /// 租户id
        /// </summary>
        public int TenantNumId { get; set; }

        /// <summary>
        /// 是否admin(超级管理员，没有租户限制)
        /// </summary>
        public bool IsSuperAdmin { get; set; }

        /// <summary>
        /// 是否管理员
        /// 1=管理员
        /// 0=普通用户
        /// </summary>
        public bool IsManager { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 是否测试
        /// </summary>
        public bool IsTest { get; set; }

        /// <summary>
        /// 姓别：1=男，2=女，-1 = 不选
        /// </summary>
        public int Sex { get; set; } = -1;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 角色id
        /// </summary>
        public string RoleId { get; set; }

        /// <summary>
        /// 最后一次登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

    }
}
