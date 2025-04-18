using EasyIotSharp.Core.Services.IO;
using EasyIotSharp.Core.Services.Tenant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UPrime.WebApi;
using EasyIotSharp.Core.Dto.IO.Params;

namespace EasyIotSharp.API.Controllers
{
    public class Util4yController : ApiControllerBase
    {
        private readonly IMinIOFileService _minIOFileService;

        public Util4yController()
        {
            _minIOFileService = UPrime.UPrimeEngine.Instance.Resolve<IMinIOFileService>();
        }

        /// <summary>
        /// 媒体文件上传
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/util4y/common/mediafile/upload")]
        [DisableRequestSizeLimit]
        public async Task<UPrimeResponse<UploadMediaFileOutput>> UploadMediaFile(UploadMediaFileInput input)
        {
            UPrimeResponse<UploadMediaFileOutput> res = new UPrimeResponse<UploadMediaFileOutput>();
            res.Result = new UploadMediaFileOutput();
            var result =await _minIOFileService.UploadAsync(input.FormFile);
            res.Result.FileName = input.FormFile.FileName;
            res.Result.SourceUrl = result;
            res.Result.Size = ((decimal)input.FormFile.Length / (decimal)1024).ToString("N2") + "KB";
            return res;
        }
    }
}
