using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Dto.Project;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Project.Impl
{
    public class ProjectBaseCacheService : CachingServiceBase, IProjectBaseCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Project}ProjectBaseCacheService");
        public const string KEY_PROJECT_PROJECTBASE_QUERY = "Project:ProjectBase-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<ProjectBaseDto>> QueryProjectBase(QueryProjectBaseInput input, Func<Task<PagedResultDto<ProjectBaseDto>>> action)
        {
            var key = KEY_PROJECT_PROJECTBASE_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.THIRTY_EXPIRES_MINUTES));
        }
    }
}
