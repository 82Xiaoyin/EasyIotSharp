using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Dto.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Domain.Hardware;

namespace EasyIotSharp.Core.Services.Hardware
{
    public interface ISensorQuotaService
    {
        /// <summary>
        /// 通过id获取一条传感器类型指标
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<SensorQuotaDto> GetSensorQuota(string id);

        /// <summary>
        /// 根据条件分页查询传感器类型指标列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<SensorQuotaDto>> QuerySensorQuota(QuerySensorQuotaInput input);

        /// <summary>
        /// 添加一条传感器类型指标
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task InsertSensorQuota(InsertSensorQuotaInput input);

        /// <summary>
        /// 通过id修改一条传感器类型指标
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateSensorQuota(UpdateSensorQuotaInput input);

        /// <summary>
        /// 通过id删除一条传感器类型指标
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteSensorQuota(DeleteSensorQuotaInput input);

        /// <summary>
        /// 传感器指标列表
        /// </summary>
        /// <returns></returns>
        List<SensorQuota> GetSensorQuotaList();

        /// <summary>
        /// 从实体数据库获取信息
        /// </summary>
        /// <param name="dataRespost"></param>
        /// <returns></returns>
        Task<Response> GetSensorData(DataRespost dataRespost);
    }
}
