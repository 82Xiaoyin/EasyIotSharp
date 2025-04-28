using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.File.Params;
using EasyIotSharp.Core.Dto.File;
using System.Threading.Tasks;
using UPrime.Services.Dto;
using Microsoft.AspNetCore.Http;
using EasyIotSharp.Core.Dto.Enum;
using EasyIotSharp.Core.Dto;
using System.IO;

namespace EasyIotSharp.Core.Services.Files
{
    public interface IResourceService
    {
        /// <summary>
        /// 资源列表查询
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<ResourceDto>> QueryResources(ResourceInput input);

        /// <summary>
        /// 资源文件上传
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<string> UploadResponseInsert(ResourceInsert insert);

        /// <summary>
        /// 修改资源
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<string> UpdateResource(UpdateResourceInput input);

        /// <summary>
        /// 删除资源
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<string> DeleteResource(DeleteInput input);

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="input">下载参数</param>
        /// <returns>文件下载信息</returns>
        Task<FileDownloadDto> DownloadResource(DownloadResourceInput input);
    }
}
