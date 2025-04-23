using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto;
using System.Threading.Tasks;
using EasyIotSharp.Core.Services.Project;
using EasyIotSharp.Core.Services.Rule;
using EasyIotSharp.Core.Services.Rule.Impl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UPrime.Services.Dto;
using UPrime.WebApi;
using EasyIotSharp.API.Filters;

namespace EasyIotSharp.API.Controllers
{
    public class AutomationController : ApiControllerBase
    {
        private readonly ISceneManagementService _sceneManagementService;
        public AutomationController()
        {
            _sceneManagementService = UPrime.UPrimeEngine.Instance.Resolve<ISceneManagementService>();
        }

        #region 场景管理

        /// <summary>
        /// 通过条件分页查询报警配置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Automation/SceneManagement/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<SceneManagementDto>>> QuerySceneManagement([FromBody] SceneManagementInput input)
        {
            UPrimeResponse<PagedResultDto<SceneManagementDto>> res = new UPrimeResponse<PagedResultDto<SceneManagementDto>>();
            res.Result = await _sceneManagementService.QuerySceneManagement(input);
            return res;
        }

        /// <summary>
        /// 新增报警信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Automation/SceneManagement/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertSceneManagement([FromBody] InsertSceneManagement input)
        {
            await _sceneManagementService.InsertSceneManagement(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 修改报警信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Automation/SceneManagement/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateSceneManagement([FromBody] InsertSceneManagement input)
        {
            await _sceneManagementService.UpdateSceneManagement(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 删除报警信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Automation/SceneManagement/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteSceneManagement([FromBody] DeleteInput input)
        {
            await _sceneManagementService.DeleteSceneManagement(input);
            return new UPrimeResponse();
        }

        #endregion

    }
}