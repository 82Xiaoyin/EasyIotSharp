using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Dto.Project;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Project
{
    public interface IProjectBaseCacheService : ICacheService
    {
        /// <summary>
        /// 项目
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<ProjectBaseDto>> QueryProjectBase(QueryProjectBaseInput input, Func<Task<PagedResultDto<ProjectBaseDto>>> action);
    }
}
