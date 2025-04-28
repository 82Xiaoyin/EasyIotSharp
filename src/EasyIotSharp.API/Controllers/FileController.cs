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
        /// 通过id获取一条项目信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("/File/Resource/List")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<ResourceDto>>> QueryResources([FromBody] ResourceInput input)
        {
            UPrimeResponse<PagedResultDto<ResourceDto>> res = new UPrimeResponse<PagedResultDto<ResourceDto>>();
            res.Result = await _resourceService.QueryResources(input);
            return res;
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/File/Resource/Upload")]
        //[Authorize]
        public async Task<UPrimeResponse<string>> UploadResponseInsert(ResourceInsert insert)
        {
            UPrimeResponse<string> res = new UPrimeResponse<string>();
            res.Result = await _resourceService.UploadResponseInsert(insert);
            return res;
        }

        /// <summary>
        /// 文件修改
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/File/Resource/Update")]
        //[Authorize]
        public async Task<UPrimeResponse<string>> UpdateResource(UpdateResourceInput insert)
        {
            UPrimeResponse<string> res = new UPrimeResponse<string>();
            res.Result = await _resourceService.UpdateResource(insert);
            return res;
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/File/Resource/Delete")]
        //[Authorize]
        public async Task<UPrimeResponse<string>> DeleteResource([FromBody] DeleteInput insert)
        {
            UPrimeResponse<string> res = new UPrimeResponse<string>();
            res.Result = await _resourceService.DeleteResource(insert);
            return res;
        }
    }
}
