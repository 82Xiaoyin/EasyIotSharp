using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Hardware;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Dto.Hardware;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Dto.Project;

namespace EasyIotSharp.Core.Caching.Project.Impl
{
    public class ClassificationCacheService : CachingServiceBase, IClassificationCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Project}ClassificationCacheService");
        public const string KEY_PROJECT_CLASSIFICATION_QUERY = "Project:Classification-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<ClassificationDto>> QueryClassification(QueryClassificationInput input, Func<Task<PagedResultDto<ClassificationDto>>> action)
        {
            var key = KEY_PROJECT_CLASSIFICATION_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.THIRTY_EXPIRES_MINUTES));
        }
    }
}
