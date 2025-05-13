using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Hardware.Params;
using System.Threading.Tasks;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Domain.Hardware;
using EasyIotSharp.Core.Repositories.Influxdb;
using MongoDB.Bson.IO;
using EasyIotSharp.Core.Dto.Export.Params;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Minio.DataModel;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Repositories.Rule;
using EasyIotSharp.Core.Repositories.Project;
using EasyIotSharp.Core.Repositories.Project.Impl;
using System.Linq;

namespace EasyIotSharp.Core.Services.Rule.Impl
{
    /// <summary>
    /// 报警服务实现类
    /// 用于处理系统报警相关的业务逻辑
    /// </summary>
    public class AlarmsService : ServiceBase, IAlarmsService
    {
        private readonly ISensorPointRepository _sensorPointRepository;
        /// <summary>
        /// 构造函数
        /// </summary>
        public AlarmsService(ISensorPointRepository sensorPointRepository)
        {
            _sensorPointRepository = sensorPointRepository;
        }

        /// <summary>
        /// 获取报警数据
        /// </summary>
        /// <param name="input">查询参数，包含传感器ID、项目ID、时间范围等条件</param>
        /// <returns>报警数据列表</returns>
        public async Task<PagedResultDto<AlarmsDto>> GetAlarmsList(AlarmsInput input)
        {
            var list = new List<AlarmsDto>();
            var measurementName = $"alarms";


            var sensorPointList = _sensorPointRepository.GetSensorPointList();
            var sqlBuilder = new StringBuilder("select *");
            // 添加表名和条件
            sqlBuilder.Append(" from ").Append(measurementName).Append(" where 1=1");

            if (!string.IsNullOrEmpty(input.SensorPointId))
            {
                sqlBuilder.Append($" and pointid = '{input.SensorPointId}'");
            }

            if (!string.IsNullOrEmpty(input.ProjectId))
            {
                sqlBuilder.Append($" and projectid = '{input.ProjectId}'");
            }

            // 添加时间范围条件（如果有）
            if (input.StartTime.HasValue && input.EndTime.HasValue)
            {
                sqlBuilder.Append($" and time >= '{input.StartTime.Value:yyyy-MM-ddTHH:mm:ss.fffZ}' and time <= '{input.EndTime.Value:yyyy-MM-ddTHH:mm:ss.fffZ}'");
            }

            if (input.IsSort)
            {
                // 添加排序
                sqlBuilder.Append(" ORDER BY time DESC ");
            }

            var countsql = sqlBuilder.ToString();

            if (input.IsPage == true && input.PageIndex > 0 && input.PageSize > 0)
            {
                sqlBuilder.Append($" Limit {input.PageSize}  Offset {input.PageIndex - 1}");
            }

            //暂时遗弃不需要加
            //sqlBuilder.Append(" tz('Asia/Shanghai');");

            // 创建仓储实例
            var repository = InfluxdbRepositoryFactory.Create<Dictionary<string, object>>(
                measurementName: measurementName,
                tenantDatabase: input.Abbreviation == null ? ContextUser.TenantAbbreviation : input.Abbreviation
            );

            // 查询数据
            var data = await repository.GetAsync(sqlBuilder.ToString());
            // 检查返回数据是否为空
            if (data == null || data.Values == null || data.Values.Count == 0 || data.Columns == null)
            {
                return new PagedResultDto<AlarmsDto>();
            }
            var countdata = await repository.GetAsync(countsql);

            var dicList = new List<Dictionary<string, object>>();
            if (data != null)
            {
                foreach (var item in data.Values)
                {
                    var dic = new Dictionary<string, object>();
                    for (int j = 0; j < item.Count; j++)
                    {
                        var rawTimestamp = item[j];
                        // 直接使用原始时间戳进行格式化，避免转换损失精度
                        if (rawTimestamp is DateTime dateTime)
                        {
                            dic.Add(data.Columns[j], dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture));
                        }
                        else
                            dic.Add(data.Columns[j], item[j] == null ? null : item[j].ToString());
                    }
                    dicList.Add(dic);
                }
            }
            list = ConvertToDtoList(dicList);
            list.ForEach(f => f.pointName = sensorPointList.Where(w => w.Id == f.pointid).FirstOrDefault()?.Name ?? "");
            var result = new PagedResultDto<AlarmsDto>();
            result.Items = list;
            result.TotalCount = countdata.Values.Count;
            return result;
        }

        /// <summary>
        /// 获取报警数据
        /// </summary>
        /// <param name="input">查询参数，包含传感器ID、项目ID、时间范围等条件</param>
        /// <returns>报警数据列表</returns>
        public async Task<List<AlarmsDto>> GetAlarmsData(AlarmsInput input)
        {
            var list = new List<AlarmsDto>();
            var measurementName = $"alarms";

            var sqlBuilder = new StringBuilder("select *");
            // 添加表名和条件
            sqlBuilder.Append(" from ").Append(measurementName).Append(" where 1=1");

            if (!string.IsNullOrEmpty(input.SensorPointId))
            {
                sqlBuilder.Append($" and pointid = '{input.SensorPointId}'");
            }

            if (!string.IsNullOrEmpty(input.ProjectId))
            {
                sqlBuilder.Append($" and projectid = '{input.ProjectId}'");
            }

            // 添加时间范围条件（如果有）
            if (input.StartTime.HasValue && input.EndTime.HasValue)
            {
                sqlBuilder.Append($" and time >= '{input.StartTime.Value:yyyy-MM-ddTHH:mm:ss.fffZ}' and time <= '{input.EndTime.Value:yyyy-MM-ddTHH:mm:ss.fffZ}'");
            }

            if (input.IsSort)
            {
                // 添加排序
                sqlBuilder.Append(" ORDER BY time DESC ");
            }

            if (input.IsPage == true && input.PageIndex > 0 && input.PageSize > 0)
            {
                sqlBuilder.Append($" Limit {input.PageSize}  Offset {input.PageIndex - 1}");
            }

            //暂时遗弃不需要加
            //sqlBuilder.Append(" tz('Asia/Shanghai');");

            // 创建仓储实例
            var repository = InfluxdbRepositoryFactory.Create<Dictionary<string, object>>(
                measurementName: measurementName,
                tenantDatabase: input.Abbreviation == null ? ContextUser.TenantAbbreviation : input.Abbreviation
            );

            // 查询数据
            var data = await repository.GetAsync(sqlBuilder.ToString());

            var dicList = new List<Dictionary<string, object>>();

            if (data != null)
            {
                foreach (var item in data.Values)
                {
                    var dic = new Dictionary<string, object>();
                    for (int j = 0; j < item.Count; j++)
                    {
                        var rawTimestamp = item[j];
                        // 直接使用原始时间戳进行格式化，避免转换损失精度
                        if (rawTimestamp is DateTime dateTime)
                        {
                            dic.Add(data.Columns[j], dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture));
                        }
                        else
                            dic.Add(data.Columns[j], item[j] == null ? null : item[j].ToString());
                    }
                    dicList.Add(dic);
                }
                list = ConvertToDtoList(dicList);
            }

            return list;
        }

        /// <summary>
        /// 更新报警数据
        /// </summary>
        /// <param name="input">报警数据传输对象，包含需要更新的报警信息</param>
        /// <returns>异步任务</returns>
        /// <remarks>
        /// 该方法用于更新报警记录，包括以下字段：
        /// - 时间戳
        /// - 确认状态（默认为true）
        /// - 报警等级
        /// - 报警类型
        /// - 条件详情
        /// - 报警消息
        /// - 通知状态
        /// - 传感器信息
        /// - 项目信息
        /// - 规则信息
        /// - 阈值信息
        /// </remarks>
        public async Task UpdateAlarms(AlarmsDto input)
        {
            var tenantAbbreviation = ContextUser.TenantAbbreviation;
            var dic = new Dictionary<string, object>();
            dic.Add("time", input.time);
            dic.Add("acknowledged", true);
            dic.Add("alarmlevel", input.alarmlevel);
            dic.Add("alarmtype", input.alarmtype);
            dic.Add("conditiondetails", input.conditiondetails);
            dic.Add("message", input.message);
            dic.Add("notified", input.notified);
            dic.Add("pointid", input.pointid);
            //dic.Add("sensorid", input.sensorid);
            //dic.Add("sensorname", input.sensorname);
            dic.Add("pointtype", input.pointtype);
            dic.Add("projectid", input.projectid);
            dic.Add("remark", input.remark);
            dic.Add("ruleid", input.ruleid);
            dic.Add("rulename", input.rulename);
            dic.Add("targetid", input.targetid);
            dic.Add("threshold_operator", input.threshold_operator);
            dic.Add("threshold_value", input.threshold_value);
            dic.Add("trigger_param", input.trigger_param);

            float triggerValue;
            if (float.TryParse(input.trigger_value?.ToString(), out triggerValue))
            {
                dic.Add("trigger_value", triggerValue);
            }
            else
            {
                // 如果转换失败，可以设置一个默认值或者跳过该字段
                dic.Add("trigger_value", 0.0f);
            }


            var measurementName = "alarms";
            var dataPoints = new List<Dictionary<string, object>> { dic };



            // 创建仓储实例
            var repository = InfluxdbRepositoryFactory.Create<Dictionary<string, object>>(
                measurementName: measurementName,
                tenantDatabase: tenantAbbreviation
            );
            // 1. 删除原有数据
            var deleteQuery = $"DELETE FROM alarms WHERE time = '{input.time:yyyy-MM-ddTHH:mm:ss.fffZ}'";
            // 执行删除
            await repository.DeleteAsync(deleteQuery);


            await repository.BulkInsertAsync(dataPoints);
        }

        /// <summary>
        /// 将字典列表转换为报警DTO列表
        /// </summary>
        /// <param name="list">包含报警数据的字典列表</param>
        /// <returns>转换后的报警DTO列表</returns>
        /// <remarks>
        /// 该方法通过反射将字典中的数据映射到AlarmsDto对象的属性中
        /// 支持自动类型转换，将字符串值转换为属性对应的类型
        /// </remarks>
        public static List<AlarmsDto> ConvertToDtoList(List<Dictionary<string, object>> list)
        {
            var alarms = new List<AlarmsDto>();

            foreach (var dict in list)
            {
                var alarm = new AlarmsDto();

                foreach (var property in typeof(AlarmsDto).GetProperties())
                {
                    if (dict.TryGetValue(property.Name, out object value))
                    {
                        // 类型转换（如 string → int/DateTime）
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(alarm, convertedValue);
                    }
                }

                alarms.Add(alarm);
            }

            return alarms;
        }
    }
}