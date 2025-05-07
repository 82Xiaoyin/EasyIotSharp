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
        private readonly IRuleChainService _ruleChainService;
        public AutomationController()
        {
            _sceneManagementService = UPrime.UPrimeEngine.Instance.Resolve<ISceneManagementService>();
            _ruleChainService = UPrime.UPrimeEngine.Instance.Resolve<IRuleChainService>();
        }

        #region 场景管理

        /// <summary>
        /// 通过条件分页查询场景管理
        /// </summary>
        /// <param name="input">入参</param>
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
        /// 新增场景管理
        /// </summary>
        /// <param name="input">入参</param>
        /// <returns></returns>
        [HttpPost("/Automation/SceneManagement/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertSceneManagement([FromBody] InsertSceneManagement input)
        {
            await _sceneManagementService.InsertSceneManagement(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 修改场景管理
        /// </summary>
        /// <param name="input">入参</param>
        /// <returns></returns>
        [HttpPost("/Automation/SceneManagement/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateSceneManagement([FromBody] InsertSceneManagement input)
        {
            await _sceneManagementService.UpdateSceneManagement(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 删除场景管理
        /// </summary>
        /// <param name="input">入参</param>
        /// <returns></returns>
        [HttpPost("/Automation/SceneManagement/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteSceneManagement([FromBody] DeleteInput input)
        {
            await _sceneManagementService.DeleteSceneManagement(input);
            return new UPrimeResponse();
        }

        #endregion


        #region 场景联动

        /// <summary>
        /// 通过条件分页查询场景联动
        /// </summary>
        /// <param name="input">入参</param>
        /// <returns></returns>
        [HttpPost("/Automation/RuleChain/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<RuleChainDto>>> QueryRuleChain([FromBody] SceneManagementInput input)
        {
            UPrimeResponse<PagedResultDto<RuleChainDto>> res = new UPrimeResponse<PagedResultDto<RuleChainDto>>();
            res.Result = await _ruleChainService.QueryRuleChain(input);
            return res;
        }

        /// <summary>
        /// 新增场景联动
        /// </summary>
        /// <param name="input">入参</param>
        /// <returns></returns>
        [HttpPost("/Automation/RuleChain/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertRuleChain([FromBody] InsertRuleChain input)
        {
            await _ruleChainService.InsertRuleChain(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 修改场景联动
        /// </summary>
        /// <param name="input">入参</param>
        /// <returns></returns>
        [HttpPost("/Automation/RuleChain/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateRuleChain([FromBody] InsertRuleChain input)
        {
            await _ruleChainService.UpdateRuleChain(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 删除场景联动
        /// </summary>
        /// <param name="input">入参</param>
        /// <returns></returns>
        [HttpPost("/Automation/RuleChain/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteRuleChain([FromBody] DeleteInput input)
        {
            await _ruleChainService.DeleteRuleChain(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 修改场景联动状态
        /// </summary>
        /// <param name="input">入参</param>
        /// <returns></returns>
        [HttpPost("/Automation/RuleChain/UpdateState")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateRuleChainState([FromBody] InsertRuleChain input)
        {
            await _ruleChainService.UpdateRuleChainState(input);
            return new UPrimeResponse();
        }

        #endregion
    }
}