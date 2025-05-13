using EasyIotSharp.Core.Dto.Project;
using System.Threading.Tasks;
using EasyIotSharp.Core.Services.Files;
using EasyIotSharp.Core.Services.Project;
using EasyIotSharp.Core.Services.Project.Impl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UPrime.WebApi;
using EasyIotSharp.API.Filters;
using EasyIotSharp.Core.Dto.File.Params;
using EasyIotSharp.Core.Dto.File;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Services.Hardware;
using EasyIotSharp.Core.Dto.Enum;
using EasyIotSharp.Core.Dto;
using System;

namespace EasyIotSharp.API.Controllers
{
    public class FileController : ApiControllerBase
    {
        private readonly IResourceService _resourceService;
        public FileController()
        {
            _resourceService = UPrime.UPrimeEngine.Instance.Resolve<IResourceService>();
        }

        /// <summary>
        /// 资源列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/File/Resource/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<ResourceDto>>> QueryResources([FromBody] ResourceInput input)
        {
            UPrimeResponse<PagedResultDto<ResourceDto>> res = new UPrimeResponse<PagedResultDto<ResourceDto>>();
            res.Result = await _resourceService.QueryResources(input);
            return res;
        }

        /// <summary>
        /// 资源上传
        /// </summary>
        /// <param name="insert"></param>
        /// <returns></returns>
        [HttpPost("/File/Resource/Insert")]
        [Authorize]
        public async Task<UPrimeResponse<string>> UploadResponseInsert(ResourceInsert insert)
        {
            UPrimeResponse<string> res = new UPrimeResponse<string>();
            res.Result = await _resourceService.UploadResponseInsert(insert);
            return res;
        }

        /// <summary>
        /// 资源修改
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/File/Resource/Update")]
        [Authorize]
        public async Task<UPrimeResponse<string>> UpdateResource(UpdateResourceInput input)
        {
            UPrimeResponse<string> res = new UPrimeResponse<string>();
            res.Result = await _resourceService.UpdateResource(input);
            return res;
        }

        /// <summary>
        /// 资源删除
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/File/Resource/Delete")]
        [Authorize]
        public async Task<UPrimeResponse<string>> DeleteResource([FromBody] DeleteInput input)
        {
            UPrimeResponse<string> res = new UPrimeResponse<string>();
            res.Result = await _resourceService.DeleteResource(input);
            return res;
        }

        /// <summary> 
        /// 资源下载 
        /// </summary> 
        /// <param name="input"></param> 
        /// <returns></returns> 
        [HttpPost("/File/Resource/Download")]
        [Authorize]
        public async Task<IActionResult> DownloadFile([FromBody] DownloadResourceInput input)
        {
            try
            {
                // 验证输入
                if (input == null || string.IsNullOrEmpty(input.Id))
                {
                    return BadRequest("资源ID不能为空");
                }

                var result = await _resourceService.DownloadResource(input);

                // 验证结果
                if (result == null || result.FileStream == null)
                {
                    return NotFound("找不到指定的资源文件");
                }

                // 验证文件流
                if (result.FileStream.Length == 0)
                {
                    return NotFound("资源文件为空");
                }

                return File(result.FileStream, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "下载资源文件时发生错误");
            }
        }
    }
}
