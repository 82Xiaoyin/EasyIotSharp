using EasyIotSharp.API.Filters;
using EasyIotSharp.Core.Dto.Gateways;
using EasyIotSharp.Core.Dto.Project;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Services.Project;
using EasyIotSharp.Core.Services.Tenant;
using EasyIotSharp.GateWay.Core.Socket;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UPrime.Services.Dto;
using UPrime.WebApi;

namespace EasyIotSharp.API.Controllers
{
    public class ProjectController : ApiControllerBase
    {
        private readonly IProjectBaseService _projectBaseService;
        private readonly IClassificationService _classificationService;
        private readonly IGatewayService _gatewayService;
        private readonly IGatewayProtocolService _gatewayProtocolService;
        private readonly ISensorPointService _sensorPointService;

        public ProjectController()
        {
            _projectBaseService = UPrime.UPrimeEngine.Instance.Resolve<IProjectBaseService>();
            _classificationService = UPrime.UPrimeEngine.Instance.Resolve<IClassificationService>();
            _gatewayService = UPrime.UPrimeEngine.Instance.Resolve<IGatewayService>();
            _gatewayProtocolService= UPrime.UPrimeEngine.Instance.Resolve<IGatewayProtocolService>();
            _sensorPointService = UPrime.UPrimeEngine.Instance.Resolve<ISensorPointService>();
        }

        #region 项目

        /// <summary>
        /// 通过id获取一条项目信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("/Project/ProjectBase/Get")]
        [Authorize]
        public async Task<UPrimeResponse<ProjectBaseDto>> GetProjectBase(string id)
        {
            UPrimeResponse<ProjectBaseDto> res = new UPrimeResponse<ProjectBaseDto>();
            res.Result = await _projectBaseService.GetProjectBase(id);
            return res;
        }

        /// <summary>
        /// 通过条件分页查询项目信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/ProjectBase/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<ProjectBaseDto>>> QueryProjectBase([FromBody]QueryProjectBaseInput input)
        {
            UPrimeResponse<PagedResultDto<ProjectBaseDto>> res = new UPrimeResponse<PagedResultDto<ProjectBaseDto>>();
            res.Result = await _projectBaseService.QueryProjectBase(input);
            return res;
        }

        /// <summary>
        /// 添加一条项目信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/ProjectBase/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertProjectBase([FromBody]InsertProjectBaseInput input)
        {
            await _projectBaseService.InsertProjectBase(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id修改一条项目信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/ProjectBase/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateProjectBase([FromBody] UpdateProjectBaseInput input)
        {
            await _projectBaseService.UpdateProjectBase(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id修改一条项目信息状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/ProjectBase/UpdateState")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateStateProjectBase([FromBody] UpdateStateProjectBaseInput input)
        {
            await _projectBaseService.UpdateStateProjectBase(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id删除一条项目信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/ProjectBase/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteProjectBase([FromBody]DeleteProjectBaseInput input)
        {
            await _projectBaseService.DeleteProjectBase(input);
            return new UPrimeResponse();
        }

        #endregion

        #region 项目分类

        /// <summary>
        /// 通过id获取一条项目分类信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("/Project/Classification/Get")]
        [Authorize]
        public async Task<UPrimeResponse<ClassificationDto>> GetClassification(string id)
        {
            UPrimeResponse<ClassificationDto> res = new UPrimeResponse<ClassificationDto>();
            res.Result = await _classificationService.GetClassification(id);
            return res;
        }

        /// <summary>
        /// 通过条件分页查询项目分类信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/Classification/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<ClassificationDto>>> QueryClassification([FromBody] QueryClassificationInput input)
        {
            UPrimeResponse<PagedResultDto<ClassificationDto>> res = new UPrimeResponse<PagedResultDto<ClassificationDto>>();
            res.Result = await _classificationService.QueryClassification(input);
            return res;
        }

        /// <summary>
        /// 添加一条项目分类信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/Classification/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertClassification([FromBody] InsertClassificationInput input)
        {
            await _classificationService.InsertClassification(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id修改一条项目分类信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/Classification/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateClassification([FromBody] UpdateClassificationInput input)
        {
            await _classificationService.UpdateClassification(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id删除一条项目分类信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/Classification/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteClassification([FromBody] DeleteClassificationInput input)
        {
            await _classificationService.DeleteClassification(input);
            return new UPrimeResponse();
        }

        #endregion

        #region 网关

        /// <summary>
        /// 通过id获取一条网关信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("/Project/Gateway/Get")]
        [Authorize]
        public async Task<UPrimeResponse<GatewayDto>> GetGateway(string id)
        {
            UPrimeResponse<GatewayDto> res = new UPrimeResponse<GatewayDto>();
            res.Result = await _gatewayService.GetGateway(id);
            return res;
        }

        /// <summary>
        /// 通过条件分页查询网关信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/Gateway/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<GatewayDto>>> QueryGateway([FromBody] QueryGatewayInput input)
        {
            UPrimeResponse<PagedResultDto<GatewayDto>> res = new UPrimeResponse<PagedResultDto<GatewayDto>>();
            res.Result = await _gatewayService.QueryGateway(input);
            return res;
        }

        /// <summary>
        /// 添加一条网关信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/Gateway/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertGateway([FromBody] InsertGatewayInput input)
        {
            await _gatewayService.InsertGateway(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id修改一条网关信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/Gateway/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateGateway([FromBody] UpdateGatewayInput input)
        {
            await _gatewayService.UpdateGateway(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id删除一条网关信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/Gateway/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteGateway([FromBody] DeleteGatewayInput input)
        {
            await _gatewayService.DeleteGateway(input);
            return new UPrimeResponse();
        }

        #endregion

        #region 网关协议

        /// <summary>
        /// 通过id获取一条网关协议信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("/Project/GatewayProtocol/Get")]
        [Authorize]
        public async Task<UPrimeResponse<GatewayProtocolDto>> GetGatewayProtocol(string id)
        {
            UPrimeResponse<GatewayProtocolDto> res = new UPrimeResponse<GatewayProtocolDto>();
            res.Result = await _gatewayProtocolService.GetGatewayProtocol(id);
            return res;
        }

        /// <summary>
        /// 通过条件分页查询网关协议信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/GatewayProtocol/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<GatewayProtocolDto>>> QueryGatewayProtocol([FromBody] QueryGatewayProtocolInput input)
        {
            UPrimeResponse<PagedResultDto<GatewayProtocolDto>> res = new UPrimeResponse<PagedResultDto<GatewayProtocolDto>>();
            res.Result = await _gatewayProtocolService.QueryGatewayProtocol(input);
            return res;
        }

        /// <summary>
        /// 添加一条网关协议信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/GatewayProtocol/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertGatewayProtocol([FromBody] InsertGatewayProtocolInput input)
        {
            await _gatewayProtocolService.InsertGatewayProtocol(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id修改一条网关协议信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/GatewayProtocol/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateGatewayProtocol([FromBody] UpdateGatewayProtocolInput input)
        {
            await _gatewayProtocolService.UpdateGatewayProtocol(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id删除一条网关协议信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/GatewayProtocol/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteGatewayProtocol([FromBody] DeleteGatewayProtocolInput input)
        {
            await _gatewayProtocolService.DeleteGatewayProtocol(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过网关id获取网关连接日志
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/GatewayProtocolLogs/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<GatewayConnectionInfo>>> GatewayProtocolLogs([FromBody] QueryGatewayProtocolInput input)
        {
            UPrimeResponse<PagedResultDto<GatewayConnectionInfo>> res = new UPrimeResponse<PagedResultDto<GatewayConnectionInfo>>();
            var ls = GatewayConnectionManager.Instance.GetAllRegisteredGateways();
            var connections = GatewayConnectionManager.Instance.GetAllRegisteredGateways().Where(x => x.GatewayId == input.GatewayId).ToList();
            res.Result = new PagedResultDto<GatewayConnectionInfo>() { TotalCount = connections.Count, Items = connections };
            return res;
        }

        #endregion

        #region 测点

        /// <summary>
        /// 通过id获取一条测点信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("/Project/SensorPoint/Get")]
        [Authorize]
        public async Task<UPrimeResponse<SensorPointDto>> GetSensorPoint(string id)
        {
            UPrimeResponse<SensorPointDto> res = new UPrimeResponse<SensorPointDto>();
            res.Result = await _sensorPointService.GetSensorPoint(id);
            return res;
        }

        /// <summary>
        /// 通过条件分页查询测点列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/SensorPoint/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<SensorPointDto>>> QuerySensorPoint([FromBody] QuerySensorPointInput input)
        {
            UPrimeResponse<PagedResultDto<SensorPointDto>> res = new UPrimeResponse<PagedResultDto<SensorPointDto>>();
            res.Result = await _sensorPointService.QuerySensorPoint(input);
            return res;
        }

        /// <summary>
        /// 添加一条测点信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/SensorPoint/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertSensorPoint([FromBody] InsertSensorPointInput input)
        {
            await _sensorPointService.InsertSensorPoint(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id修改一条测点信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/SensorPoint/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateSensorPoint([FromBody] UpdateSensorPointInput input)
        {
            await _sensorPointService.UpdateSensorPoint(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id删除一条测点
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Project/SensorPoint/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteSensorPoint([FromBody] DeleteSensorPointInput input)
        {
            await _sensorPointService.DeleteSensorPoint(input);
            return new UPrimeResponse();
        }

        /// <summary>
        ///  首页在线告警echart图
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Project/SensorPoint/Chart")]
        [Authorize]
        public async Task<UPrimeResponse<SensorPointChart>> QuerySensorPointChart([FromBody] ChartInput input)
        {
            UPrimeResponse<SensorPointChart> res = new UPrimeResponse<SensorPointChart>();
            res.Result = await _sensorPointService.QuerySensorPointChart(input);
            return res;
        }

        #endregion

    }
}
