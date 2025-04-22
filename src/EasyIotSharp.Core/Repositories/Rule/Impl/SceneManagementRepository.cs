using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Repositories.Mysql;
using System.Collections.Generic;
using System.Threading.Tasks;
using LinqKit;

namespace EasyIotSharp.Core.Repositories.Rule.Impl
{
    public class SceneManagementRepository : MySqlRepositoryBase<SceneManagement, string>, ISceneManagementRepository
    {
        public SceneManagementRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        public async Task<(int totalCount, List<SceneManagementDto> items)> Query(
            string keyword,
            int pageIndex,
            int pageSize)
        {
            var predicate = PredicateBuilder.New<SceneManagement>(x => x.IsDelete == false);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                predicate = predicate.And(x => x.SceneName.Contains(keyword));
            }

            var totalCount = await CountAsync(predicate);
            if (totalCount == 0)
            {
                return (0, new List<SceneManagementDto>());
            }

            var items = await Client.Queryable<SceneManagement>()
                .Where(predicate)
                .OrderByDescending(x => x.CreationTime)
                .Select<SceneManagementDto>()
                .ToPageListAsync(pageIndex, pageSize);

            return (totalCount, items);
        }
    }
}