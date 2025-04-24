using EasyIotSharp.Core.Domain.TenantAccount;
using Elasticsearch.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Tenant.Params
{
    public class InsertTenantInput
    {
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
