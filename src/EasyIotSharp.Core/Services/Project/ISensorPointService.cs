using EasyIotSharp.Core.Dto.Project;
using EasyIotSharp.Core.Dto.Project.Params;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Project
{
    public interface ISensorPointService
    {
        /// <summary>
        /// 通过id获取一条测点信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<SensorPointDto> GetSensorPoint(string id);

        /// <summary>
        /// 根据条件分页查询测点列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<SensorPointDto>> QuerySensorPoint(QuerySensorPointInput input);

        /// <summary>
        /// 添加一条测点信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task InsertSensorPoint(InsertSensorPointInput input);

        /// <summary>
        /// 根据id修改一条测点信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateSensorPoint(UpdateSensorPointInput input);

        /// <summary>
        /// 通过id删除一条测点信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteSensorPoint(DeleteSensorPointInput input);

        /// <summary>
        /// echat图
        /// </summary>
        /// <returns></returns>
        Task<SensorPointChart> QuerySensorPointChart(ChartInput input);
    }
}
