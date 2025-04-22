using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Repositories.Mysql;

namespace EasyIotSharp.Core.Repositories.Rule
{
    public interface IAlarmsConfigRepository : IMySqlRepositoryBase<AlarmsConfig, string>
    {
        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<(int totalCount, List<AlarmsConfigDto> items)> Query(
            int pageIndex,
            int pageSize);
    }
}
