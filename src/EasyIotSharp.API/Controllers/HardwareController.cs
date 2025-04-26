using EasyIotSharp.API.Filters;
using EasyIotSharp.Core.Dto.Hardware;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Services.Hardware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UPrime.Services.Dto;
using UPrime.WebApi;

namespace EasyIotSharp.API.Controllers
{
    public class HardwareController : ApiControllerBase
    {
        private readonly IProtocolService _protocolService;
        private readonly IProtocolConfigService _protocolConfigService;
        private readonly ISensorService _sensorService;
        private readonly ISensorQuotaService _sensorQuotaService;

        public HardwareController()
        {
            _protocolService = UPrime.UPrimeEngine.Instance.Resolve<IProtocolService>();
            _protocolConfigService= UPrime.UPrimeEngine.Instance.Resolve<IProtocolConfigService>();
            _sensorService = UPrime.UPrimeEngine.Instance.Resolve<ISensorService>();
            _sensorQuotaService = UPrime.UPrimeEngine.Instance.Resolve<ISensorQuotaService>();
        }

        #region 协议

        /// <summary>
        /// 通过id获取一条协议信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Protocol/Get")]
        [Authorize]
        public async Task<UPrimeResponse<ProtocolDto>> GetProtocol(string id)
        {
            UPrimeResponse<ProtocolDto> res = new UPrimeResponse<ProtocolDto>();
            res.Result = await _protocolService.GetProtocol(id);
            return res;
        }

        /// <summary>
        /// 通过条件分页查询协议信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Protocol/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<ProtocolDto>>> QueryProtocol([FromBody] QueryProtocolInput input)
        {
            UPrimeResponse<PagedResultDto<ProtocolDto>> res = new UPrimeResponse<PagedResultDto<ProtocolDto>>();
            res.Result = await _protocolService.QueryProtocol(input);
            return res;
        }

        /// <summary>
        /// 添加一条协议信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Protocol/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertProtocol([FromBody] InsertProtocolInput input)
        {
            await _protocolService.InsertProtocol(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id修改一条协议信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Protocol/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateProtocol([FromBody] UpdateProtocolInput input)
        {
            await _protocolService.UpdateProtocol(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id删除一条协议信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Protocol/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteProtocol([FromBody] DeleteProtocolInput input)
        {
            await _protocolService.DeleteProtocol(input);
            return new UPrimeResponse();
        }

        #endregion

        #region 协议配置

        /// <summary>
        /// 通过id获取一条协议配置信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Protocol/Config/Get")]
        [Authorize]
        public async Task<UPrimeResponse<ProtocolConfigDto>> GetProtocolConfig(string id)
        {
            UPrimeResponse<ProtocolConfigDto> res = new UPrimeResponse<ProtocolConfigDto>();
            res.Result = await _protocolConfigService.GetProtocolConfig(id);
            return res;
        }

        /// <summary>
        /// 通过条件分页查询协议配置信息列表
        /// </summary>.
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Protocol/Config/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<QueryProtocolConfigByProtocolIdOutput>>> QueryProtocolConfig([FromBody]QueryProtocolConfigInput input)
        {
            UPrimeResponse<PagedResultDto<QueryProtocolConfigByProtocolIdOutput>> res = new UPrimeResponse<PagedResultDto<QueryProtocolConfigByProtocolIdOutput>>();
            res.Result = await _protocolConfigService.QueryProtocolConfig(input);
            return res;
        }

        /// <summary>
        /// 添加一条协议配置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Protocol/Config/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertProtocolConfig([FromBody] InsertProtocolConfigInput input)
        {
            await _protocolConfigService.InsertProtocolConfig(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id修改一条协议配置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Protocol/Config/Update")]
        [Authorize]
        public async  Task<UPrimeResponse> UpdateProtocolConfig([FromBody] UpdateProtocolConfigInput input)
        {
            await _protocolConfigService.UpdateProtocolConfig(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id删除一条协议配置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Protocol/Config/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteProtocolConfig([FromBody]DeleteProtocolConfigInput input)
        {
            await _protocolConfigService.DeleteProtocolConfig(input);
            return new UPrimeResponse();
        }

        #endregion

        #region 传感器类型

        /// <summary>
        /// 通过id获取一条传感器类型信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Sensor/Get")]
        [Authorize]
        public async Task<UPrimeResponse<SensorDto>> GetSensor(string id)
        {
            UPrimeResponse<SensorDto> res = new UPrimeResponse<SensorDto>();
            res.Result = await _sensorService.GetSensor(id);
            return res;
        }

        /// <summary>
        /// 通过条件分页查询传感器信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Sensor/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<SensorDto>>> QuerySensor([FromBody] QuerySensorInput input)
        {
            UPrimeResponse<PagedResultDto<SensorDto>> res = new UPrimeResponse<PagedResultDto<SensorDto>>();
            res.Result = await _sensorService.QuerySensor(input);
            return res;
        }

        /// <summary>
        /// 添加一条传感器类型信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Sensor/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertSensor([FromBody] InsertSensorInput input)
        {
            await _sensorService.InsertSensor(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id修改一条传感器类型信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Sensor/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateSensor([FromBody] UpdateSensorInput input)
        {
            await _sensorService.UpdateSensor(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id删除一条传感器分类信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Sensor/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteSensor([FromBody] DeleteSensorInput input)
        {
            await _sensorService.DeleteSensor(input);
            return new UPrimeResponse();
        }

        #endregion

        #region 传感器类型指标

        /// <summary>
        /// 通过id获取一条传感器类型指标
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Sensor/Quota/Get")]
        [Authorize]
        public async Task<UPrimeResponse<SensorQuotaDto>> GetSensorQuota(string id)
        {
            UPrimeResponse<SensorQuotaDto> res = new UPrimeResponse<SensorQuotaDto>();
            res.Result = await _sensorQuotaService.GetSensorQuota(id);
            return res;
        }

        /// <summary>
        /// 根据条件分页查询传感器类型指标列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Sensor/Quota/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<SensorQuotaDto>>> QuerySensorQuota([FromBody] QuerySensorQuotaInput input)
        {
            UPrimeResponse<PagedResultDto<SensorQuotaDto>> res = new UPrimeResponse<PagedResultDto<SensorQuotaDto>>();
            res.Result = await _sensorQuotaService.QuerySensorQuota(input);
            return res;
        }

        /// <summary>
        /// 添加一条传感器类型指标
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Sensor/Quota/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertSensorQuota([FromBody] InsertSensorQuotaInput input)
        {
            await _sensorQuotaService.InsertSensorQuota(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id修改一条传感器类型指标
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Sensor/Quota/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateSensorQuota([FromBody] UpdateSensorQuotaInput input)
        {
            await _sensorQuotaService.UpdateSensorQuota(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 通过id删除一条传感器类型指标
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Sensor/Quota/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteSensorQuota([FromBody] DeleteSensorQuotaInput input)
        {
            await _sensorQuotaService.DeleteSensorQuota(input);
            return new UPrimeResponse();
        }

        #endregion

        /// <summary>
        /// 设备数据列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Hardware/Sensor/Data")]
        [Authorize]
        public async Task<UPrimeResponse<Response>> GetResponse([FromBody] DataRespost dataRespost)
        {
            UPrimeResponse<Response> res = new UPrimeResponse<Response>();
            res.Result = await _sensorQuotaService.GetSensorData(dataRespost);
            return res;

        }
    }
}
