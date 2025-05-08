using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Gateways;

namespace EasyIotSharp.Core.Caching.Gateways
{
    public interface IRegisteredGatewayCacheService : ICacheService
    {
        /// <summary>
        /// 获取网关列表
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        List<GatewayConnectionInfo> GetAllRegisteredGateways(Func<List<GatewayConnectionInfo>> action);
        /// <summary>
        /// 清除缓存
        /// </summary>
        void Clear();
    }
}
