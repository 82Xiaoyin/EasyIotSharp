using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Domain.Proejct;

namespace EasyIotSharp.Core.Caching.Project
{
    public interface ISensorPointBaseCacheService : ICacheService
    {
        /// <summary>
        /// 列表数据
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        List<SensorPoint> GetSensorPointBase(Func<List<SensorPoint>> action);
    }
}
