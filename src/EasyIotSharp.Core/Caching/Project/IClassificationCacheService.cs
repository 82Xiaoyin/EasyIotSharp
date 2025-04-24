using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Dto.Project;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Project
{
    public interface IClassificationCacheService : ICacheService
    {
        /// <summary>
        /// 获取项目分类
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<ClassificationDto>> QueryClassification(QueryClassificationInput input, Func<Task<PagedResultDto<ClassificationDto>>> action);
    }
}
