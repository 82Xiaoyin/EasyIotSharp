using System.Collections.Generic;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Files;
using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Dto.Enum;
using EasyIotSharp.Core.Dto.File;
using EasyIotSharp.Core.Repositories.Mysql;
using SqlSugar;
using UPrime.AutoMapper;

namespace EasyIotSharp.Core.Repositories.Files.Impl
{
    public class ResourceRepository : MySqlRepositoryBase<Resource, string>, IResourceRepository
    {
        public ResourceRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        /// <summary>
        /// 资源列表查询
        /// </summary>
        /// <param name="State"></param>
        /// <param name="ResourceEnum"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isPage"></param>
        /// <returns></returns>
        public async Task<(int totalCount, List<ResourceDto> items)> Query(bool? State,
                                                                      ResourceEnums ResourceEnum,
                                                                      int pageIndex,
                                                                      int pageSize,
                                                                      bool isPage)
        {
            var query = Client.Queryable<Resource>()
                    .WhereIF(State.HasValue, r => r.State == State.Value)
                    .WhereIF(ResourceEnum != null, r => r.Type == (int)ResourceEnum)
                    .OrderByDescending(r => r.CreationTime)
                    .Select<ResourceDto>();

            int totalCount = 0;
            List<ResourceDto> resources;

            if (isPage)
            {
                // 分页查询
                var result = await query.ToPageListAsync(pageIndex, pageSize, totalCount);
                totalCount = await query.CountAsync();
                resources = result;
            }
            else
            {
                // 不分页，查询所有
                resources = await query.ToListAsync();
                totalCount = resources.Count;
            }

            return (totalCount, resources);
        }
    }
}
