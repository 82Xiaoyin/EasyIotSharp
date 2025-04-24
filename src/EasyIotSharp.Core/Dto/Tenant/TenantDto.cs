using EasyIotSharp.Core.Domain;
using EasyIotSharp.Core.Domain.TenantAccount;
using Elasticsearch.Net;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Tenant
{
    /// <summary>
    /// 代表一个租户实体的“DTO”
    /// </summary>
    public class TenantDto
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
        /// 租户numId
        /// </summary>
        public int NumId { get; set; }

        /// <summary>
        /// 租户名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 租户简称
        /// </summary>
        public string Abbreviation { get; set; }

        #region 基本信息

        /// <summary>
        /// 机构的（唯一的等同Id）
        /// </summary>
        public string StoreKey { get; set; }

        /// <summary>
        /// 合同名称
        /// </summary>
        public string ContractName { get; set; }

        /// <summary>
        /// 合同所属人
        /// </summary>
        public string ContractOwnerName { get; set; }

        /// <summary>
        /// 合同所属人电话
        /// </summary>
        public string ContractOwnerMobile { get; set; }

        /// <summary>
        /// 合同开始时间
        /// </summary>
        public DateTime ContractStartTime { get; set; }

        /// <summary>
        /// 合同结束时间
        /// </summary>
        public DateTime ContractEndTime { get; set; }

        /// <summary>
        /// 机构的负责人手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 机构的负责人姓名
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// 机构的Logo url
        /// </summary>
        public string StoreLogoUrl { get; set; }

        /// <summary>
        /// 机构的备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 机构的电子邮箱
        /// </summary>
        public string Email { get; set; }

        #endregion 基本信息

        #region 版本 / 帐号 / 状态

        /// <summary>
        /// 过期状态
        /// 0=待授权
        /// 1=生效
        /// 2=已过期
        /// </summary>
        public int ExpiredType
        {
            get
            {
                if (ContractStartTime.IsNull() && ContractEndTime.IsNull())
                {
                    return 0;
                }
                else if (ContractStartTime.IsNotNull() && ContractStartTime > DateTime.Now)
                {
                    return 0;
                }
                else if (ContractEndTime.IsNotNull() && ContractEndTime < DateTime.Now)
                {
                    return 2;
                }
                else if (ContractStartTime.IsNotNull()&&ContractEndTime.IsNotNull()&& ContractStartTime<DateTime.Now&& ContractEndTime>DateTime.Now)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 机构的系统管理员Id 详见 <see cref="Soldier"/>
        /// </summary>
        public string ManagerId { get; set; }

        /// <summary>
        /// 版本类型，详见 <see cref="VersionType"/>
        /// 1=入门版
        /// 2=基础版
        /// 3=旗舰版
        /// 5=全国版
        /// </summary>
        public int VersionTypeId { get; set; }

        /// <summary>
        /// 是否冻结
        /// </summary>
        public bool IsFreeze { get; set; }

        /// <summary>
        /// 冻结描述
        /// </summary>
        public string FreezeDes { get; set; }

        #endregion 版本 / 帐号 / 状态
    }
}
