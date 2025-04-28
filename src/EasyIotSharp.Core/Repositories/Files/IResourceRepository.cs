using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Export;
using EasyIotSharp.Core.Domain.Files;
using EasyIotSharp.Core.Dto.Enum;
using EasyIotSharp.Core.Dto.File;
using EasyIotSharp.Core.Repositories.Mysql;

namespace EasyIotSharp.Core.Repositories.Files
{
    public interface IResourceRepository : IMySqlRepositoryBase<Resource, string>
    {

        /// <summary>
        /// 资源表查询
        /// </summary>
        /// <param name="State"></param>
        /// <param name="ResourceEnum"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isPage"></param>
        /// <returns></returns>
        Task<(int totalCount, List<ResourceDto> items)> Query(string keyWord, bool? State,
                                                                      ResourceEnums ResourceEnum,
                                                                      int pageIndex,
                                                                      int pageSize,
                                                                      bool isPage);
    }
}
