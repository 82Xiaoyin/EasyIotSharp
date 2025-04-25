using EasyIotSharp.Core.Domain.Hardware;
using EasyIotSharp.Core.Dto.Hardware;
using EasyIotSharp.Core.Dto.Hardware.Params;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Hardware
{
    public interface ISensorService
    {
        /// <summary>
        /// 通过id获取一条传感器类型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<SensorDto> GetSensor(string id);

        /// <summary>
        /// 根据条件分页查询传感器类型列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<SensorDto>> QuerySensor(QuerySensorInput input);

        /// <summary>
        /// 添加一条传感器类型
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task InsertSensor(InsertSensorInput input);

        /// <summary>
        /// 通过id修改一条传感器类型
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateSensor(UpdateSensorInput input);

        /// <summary>
        /// 通过id删除一条传感器类型
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteSensor(DeleteSensorInput input);

        /// <summary>
        /// 传感器列表
        /// </summary>
        /// <returns></returns>
        List<Sensor> GetSensorList();
    }
}
