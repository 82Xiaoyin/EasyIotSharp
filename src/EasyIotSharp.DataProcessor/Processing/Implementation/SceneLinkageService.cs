using EasyIotSharp.Core.Services.Queue;
using EasyIotSharp.Core.Services.Rule;
using EasyIotSharp.DataProcessor.Processing.Interfaces;
using EasyIotSharp.DataProcessor.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UPrime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Dto.Rule;
using log4net;
using EasyIotSharp.Core.Repositories.Influxdb;

namespace EasyIotSharp.DataProcessor.Processing.Implementation
{
    /// <summary>
    /// 场景联动服务实现类
    /// 负责处理IoT设备数据触发的场景联动规则
    /// </summary>
    public class SceneLinkageService : ISceneLinkageService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SceneLinkageService));
        
        // 缓存项目的场景联动配置，避免频繁查询数据库
        private readonly Dictionary<string, bool> _projectSceneLinkageCache = new Dictionary<string, bool>();
        
        // 规则链服务，用于查询和执行场景联动规则
        private readonly IRuleChainService _ruleChainService;
        
        // 数据仓库服务，用于存储告警数据
         private readonly IDataRepository _dataRepository;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public SceneLinkageService(IDataRepository dataRepository)
        {
            // 通过UPrime容器解析规则链服务
            _ruleChainService = UPrimeEngine.Instance.Resolve<IRuleChainService>();
            _dataRepository = dataRepository;
        }
        
        /// <summary>
        /// 检查项目是否配置了场景联动规则
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <returns>是否存在场景联动规则</returns>
        public async Task<bool> HasSceneLinkageAsync(string projectId)
        {
            try
            {
                if (_projectSceneLinkageCache.TryGetValue(projectId, out bool hasLinkage))
                {
                    return hasLinkage;
                }
                
                var input = new SceneManagementInput
                {
                    ProjectId = projectId,
                    IsPage = false
                };
                
                var result = await _ruleChainService.QueryRuleChain(input);
                bool hasRules = result != null && result.TotalCount > 0;
                
                _projectSceneLinkageCache[projectId] = hasRules;
                
                Logger.Debug($"项目 {projectId} 场景联动规则检查结果: {(hasRules ? "存在" : "不存在")}");
                return hasRules;
            }
            catch (Exception ex)
            {
                Logger.Error("检查项目场景联动配置失败", ex);
                return false;
            }
        }
        
        /// <summary>
        /// 处理场景联动
        /// 对传入的数据点应用项目的场景联动规则
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="dataPoints">数据点列表</param>
        /// <returns>处理任务</returns>
        public async Task ProcessSceneLinkageAsync(string projectId, IEnumerable<Dictionary<string, object>> dataPoints, string TenantAbbreviation)
        {
            try
            {
                // 先检查项目是否有场景联动配置，避免不必要的处理
                if (!await HasSceneLinkageAsync(projectId))
                {
                    return;
                }
                
                // 获取项目的场景联动规则
                var input = new SceneManagementInput { ProjectId = projectId };
                var ruleChains = await _ruleChainService.QueryRuleChain(input);
                
                // 验证规则链是否有效
                if (ruleChains == null || ruleChains.Items == null || !ruleChains.Items.Any())
                {
                    Logger.Debug($"项目 {projectId} 没有找到场景联动规则");
                    return;
                }
                
                Logger.Debug($"项目 {projectId} 找到 {ruleChains.Items.Count} 条场景联动规则");
                
                // 优化：转换为列表避免多次枚举
                var dataPointsList = dataPoints.ToList();
                
                // 处理每个数据点
                foreach (var dataPoint in dataPointsList)
                {
                    // 对每条规则进行评估
                    foreach (var rule in ruleChains.Items)
                    {
                        await EvaluateRuleForDataPoint(rule, dataPoint, projectId, TenantAbbreviation);
                    }
                }
                
                Logger.Debug($"项目 {projectId} 的场景联动处理完成，共处理 {dataPointsList.Count} 个数据点");
            }
            catch (Exception ex)
            {
                Logger.Error("处理场景联动失败", ex);
            }
        }
        
        /// <summary>
        /// 评估数据点是否满足规则条件
        /// </summary>
        /// <param name="rule">规则链</param>
        /// <param name="dataPoint">数据点</param>
        /// <param name="projectId">项目ID</param>
        /// <returns>评估任务</returns>
        private async Task EvaluateRuleForDataPoint(RuleChainDto rule, Dictionary<string, object> dataPoint, string projectId,string TenantAbbreviation)
        {
            try
            {
                // 验证规则内容是否有效
                if (string.IsNullOrEmpty(rule.RuleContentJson))
                {
                    Logger.Debug($"规则 {rule.Name} 的内容为空");
                    return;
                }
                
                // 解析规则条件
                var conditionsObj = JsonConvert.DeserializeObject<JObject>(rule.RuleContentJson);
                if (conditionsObj == null || !conditionsObj.ContainsKey("conditions"))
                {
                    Logger.Debug($"规则 {rule.Name} 的条件格式无效");
                    return;
                }
                
                var conditions = conditionsObj["conditions"] as JArray;
                if (conditions == null || !conditions.Any())
                {
                    Logger.Debug($"规则 {rule.Name} 没有条件");
                    return;
                }
                
                // 检查数据点是否满足所有条件
                bool allConditionsMet = true;
                
                // 记录条件评估结果，便于调试
                var conditionResults = new List<string>();
                
                foreach (var condition in conditions)
                {
                    // 获取条件类型
                    string type = condition["type"]?.ToString();
                    string timeType = condition["timeType"]?.ToString();
                    
                    bool conditionMet = true;
                    
                    // 根据类型和时间类型进行不同的处理
                    if (type == "target")
                    {
                        // 获取目标ID和参数ID
                        string targetId = condition["targetId"]?.ToString();
                        string paramCode = condition["paramCode"]?.ToString();
                        
                        // 检查数据点是否匹配目标
                        if (!dataPoint.TryGetValue("pointId", out object pointIdObj) || 
                            pointIdObj?.ToString() != targetId)
                        {
                            conditionMet = false;
                            conditionResults.Add($"目标不匹配: 期望={targetId}, 实际={pointIdObj}");
                        }
                        else if (!string.IsNullOrEmpty(paramCode) && 
                                 dataPoint.TryGetValue(paramCode, out object paramValue))
                        {
                            // 检查参数值是否满足条件
                            string operatorType = condition["operator"]?.ToString();
                            string conditionValue = condition["value"]?.ToString();
                            
                            bool paramConditionMet = EvaluateCondition(paramValue, operatorType, conditionValue);
                            if (!paramConditionMet)
                            {
                                conditionMet = false;
                                conditionResults.Add($"参数条件不满足: {paramCode} {operatorType} {conditionValue}, 实际值={paramValue}");
                            }
                            else
                            {
                                conditionResults.Add($"参数条件满足: {paramCode} {operatorType} {conditionValue}, 实际值={paramValue}");
                            }
                        }
                        else
                        {
                            conditionMet = false;
                            conditionResults.Add($"参数不存在: {paramCode}");
                        }
                    }
                    
                    // 处理时间类型条件
                    if (!string.IsNullOrEmpty(timeType))
                    {
                        // 根据timeType处理时间相关条件
                        bool timeConditionMet = EvaluateTimeCondition(timeType, condition);
                        if (!timeConditionMet)
                        {
                            conditionMet = false;
                            conditionResults.Add($"时间条件不满足: {timeType}");
                        }
                        else
                        {
                            conditionResults.Add($"时间条件满足: {timeType}");
                        }
                    }
                    
                    // 如果任一条件不满足，则整个规则不满足
                    if (!conditionMet)
                    {
                        allConditionsMet = false;
                        break;
                    }
                }
                
                // 记录条件评估详情
                Logger.Debug($"规则 {rule.Name} 条件评估结果: {(allConditionsMet ? "满足" : "不满足")}, 详情: {string.Join(", ", conditionResults)}");
                
                // 如果所有条件都满足，执行动作
                if (allConditionsMet)
                {
                    await ExecuteRuleActions(rule, projectId, dataPoint,conditionResults, TenantAbbreviation);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"评估规则失败: {ex.Message}, 规则ID: {rule.Id}");
            }
        }
        
        /// <summary>
        /// 评估条件是否满足
        /// 支持数值比较和字符串比较
        /// </summary>
        /// <param name="actualValue">实际值</param>
        /// <param name="operatorType">操作符类型</param>
        /// <param name="expectedValue">期望值</param>
        /// <returns>条件是否满足</returns>
        private bool EvaluateCondition(object actualValue, string operatorType, string expectedValue)
        {
            try
            {
                // 空值处理
                if (actualValue == null)
                {
                    return operatorType == "eq" && string.IsNullOrEmpty(expectedValue);
                }
                
                // 转换为可比较的类型
                if (double.TryParse(actualValue.ToString(), out double actualDouble) && 
                    double.TryParse(expectedValue, out double expectedDouble))
                {
                    // 数值比较
                    switch (operatorType)
                    {
                        case "eq": return Math.Abs(actualDouble - expectedDouble) < 0.0001; // 等于
                        case "neq": return Math.Abs(actualDouble - expectedDouble) >= 0.0001; // 不等于
                        case "gt": return actualDouble > expectedDouble; // 大于
                        case "gte": return actualDouble >= expectedDouble; // 大于等于
                        case "lt": return actualDouble < expectedDouble; // 小于
                        case "lte": return actualDouble <= expectedDouble; // 小于等于
                        default: 
                            Logger.Warn($"不支持的操作符类型: {operatorType}");
                            return false;
                    }
                }
                else
                {
                    // 字符串比较
                    string actualString = actualValue.ToString() ?? string.Empty;
                    
                    switch (operatorType)
                    {
                        case "eq": return actualString == expectedValue; // 等于
                        case "neq": return actualString != expectedValue; // 不等于
                        case "contains": return actualString.Contains(expectedValue); // 包含
                        case "notcontains": return !actualString.Contains(expectedValue); // 不包含
                        default: 
                            Logger.Warn($"不支持的操作符类型: {operatorType}");
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"评估条件失败: {ex.Message}, 实际值: {actualValue}, 操作符: {operatorType}, 期望值: {expectedValue}");
                return false;
            }
        }
        
        /// <summary>
        /// 评估时间条件是否满足
        /// 支持特定时间点、时间范围和周期性时间
        /// </summary>
        /// <param name="timeType">时间类型</param>
        /// <param name="condition">条件对象</param>
        /// <returns>时间条件是否满足</returns>
        private bool EvaluateTimeCondition(string timeType, JToken condition)
        {
            try
            {
                // 获取当前时间
                DateTime now = DateTime.Now;
                
                switch (timeType)
                {
                    case "specific":
                        // 特定时间点
                        if (condition["specificTime"] != null)
                        {
                            DateTime specificTime = condition["specificTime"].ToObject<DateTime>();
                            // 检查当前时间是否在特定时间点的前后5分钟内
                            var timeDiff = Math.Abs((now - specificTime).TotalMinutes);
                            Logger.Debug($"特定时间条件: 当前时间={now}, 特定时间={specificTime}, 时间差={timeDiff}分钟");
                            return timeDiff <= 5;
                        }
                        break;
                        
                    case "range":
                        // 时间范围
                        if (condition["startTime"] != null && condition["endTime"] != null)
                        {
                            DateTime startTime = condition["startTime"].ToObject<DateTime>();
                            DateTime endTime = condition["endTime"].ToObject<DateTime>();
                            Logger.Debug($"时间范围条件: 当前时间={now}, 开始时间={startTime}, 结束时间={endTime}");
                            return now >= startTime && now <= endTime;
                        }
                        break;
                        
                    case "periodic":
                        // 周期性时间
                        if (condition["periodicType"] != null)
                        {
                            string periodicType = condition["periodicType"].ToString();
                            
                            if (periodicType == "daily" && condition["periodicTime"] != null)
                            {
                                // 每天特定时间
                                TimeSpan periodicTime = TimeSpan.Parse(condition["periodicTime"].ToString());
                                TimeSpan currentTime = now.TimeOfDay;
                                var timeDiff = Math.Abs((currentTime - periodicTime).TotalMinutes);
                                Logger.Debug($"每日周期条件: 当前时间={currentTime}, 周期时间={periodicTime}, 时间差={timeDiff}分钟");
                                return timeDiff <= 5;
                            }
                            else if (periodicType == "weekly" && condition["weekDays"] != null && condition["periodicTime"] != null)
                            {
                                // 每周特定日期和时间
                                var weekDays = condition["weekDays"].ToObject<List<int>>();
                                int currentDayOfWeek = (int)now.DayOfWeek;
                                
                                if (weekDays.Contains(currentDayOfWeek))
                                {
                                    TimeSpan periodicTime = TimeSpan.Parse(condition["periodicTime"].ToString());
                                    TimeSpan currentTime = now.TimeOfDay;
                                    var timeDiff = Math.Abs((currentTime - periodicTime).TotalMinutes);
                                    Logger.Debug($"每周周期条件: 当前星期={currentDayOfWeek}, 当前时间={currentTime}, 周期时间={periodicTime}, 时间差={timeDiff}分钟");
                                    return timeDiff <= 5;
                                }
                                else
                                {
                                    Logger.Debug($"每周周期条件: 当前星期={currentDayOfWeek}, 不在指定星期内={string.Join(",", weekDays)}");
                                }
                            }
                        }
                        break;
                        
                    default:
                        Logger.Warn($"不支持的时间类型: {timeType}");
                        break;
                }
                
                // 如果没有时间条件或时间条件不完整，默认返回true
                return string.IsNullOrEmpty(timeType);
            }
            catch (Exception ex)
            {
                Logger.Error($"评估时间条件失败: {ex.Message}, 时间类型: {timeType}");
                return false;
            }
        }
        
        /// <summary>
        /// 执行规则触发的动作
        /// </summary>
        /// <param name="rule">规则链</param>
        /// <param name="dataPoint">触发规则的数据点</param>
        /// <param name="projectId">项目ID</param>
        /// <returns>执行任务</returns>
        private async Task ExecuteRuleActions(RuleChainDto rule, string projectId, Dictionary<string, object> dataPoint, List<string> conditionResults,string TenantAbbreviation)
        {
            try
            {
                Logger.Info($"触发场景联动规则: {rule.Name}, 项目ID: {projectId}");
                
                // 保存告警数据到InfluxDB
                await SaveAlarmToInfluxDBAsync(projectId, rule, dataPoint, conditionResults, TenantAbbreviation);
                
                // TODO: 实现规则动作执行逻辑
                // 这里需要根据实际业务需求实现，可能包括：
                // 1. 发送通知（短信、邮件、推送）
                // 2. 控制设备（开关、调节参数）
                // 3. 记录事件（报警、日志）
                // 4. 调用外部API
                // 5. 触发其他规则链
                
                // 解析规则动作
                if (!string.IsNullOrEmpty(rule.AlarmsJSON))
                {
                    var ruleContent = JsonConvert.DeserializeObject<JObject>(rule.AlarmsJSON);
                    if (ruleContent != null && ruleContent.ContainsKey("actions"))
                    {
                        var actions = ruleContent["actions"] as JArray;
                        if (actions != null && actions.Any())
                        {
                            Logger.Info($"规则 {rule.Name} 包含 {actions.Count} 个动作");
                            
                            // 执行每个动作
                            foreach (var action in actions)
                            {
                                string actionType = action["type"]?.ToString();
                                Logger.Info($"执行动作: {actionType}");
                                //通知ID
                                string alarmSceneId= action["alarmSceneId"]?.ToString();
                                // 根据动作类型执行不同的操作
                                switch (actionType)
                                {
                                    case "alarm_notify":
                                        // 处理告警通知动作
                                        await ProcessAlarmNotifyAction(action, rule, dataPoint, projectId);
                                        break;
                                        
                                    case "device_control":
                                        // 控制设备
                                        await ControlDevice(action, rule, dataPoint, projectId);
                                        break;
                                        
                                    case "record":
                                        // 记录事件
                                        await RecordEvent(action, rule, dataPoint, projectId);
                                        break;
                                        
                                    default:
                                        Logger.Warn($"不支持的动作类型: {actionType}");
                                        break;
                                }
                            }
                        }
                    }
                }
                
                // 记录规则触发事件
                Logger.Info($"规则 {rule.Name} 触发，数据点: {JsonConvert.SerializeObject(dataPoint)}");
            }
            catch (Exception ex)
            {
                Logger.Error($"执行规则动作失败: {ex.Message}, 规则ID: {rule.Id}");
            }
        }
        
        /// <summary>
        /// 控制设备
        /// </summary>
        private async Task ControlDevice(JToken action, RuleChainDto rule, Dictionary<string, object> dataPoint, string projectId)
        {
            try
            {
                string deviceId = action["deviceId"]?.ToString();
                string command = action["command"]?.ToString();
                
                Logger.Info($"控制设备: 设备ID={deviceId}, 命令={command}");
                
                // TODO: 实现具体的设备控制逻辑
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.Error($"控制设备失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 记录事件
        /// </summary>
        private async Task RecordEvent(JToken action, RuleChainDto rule, Dictionary<string, object> dataPoint, string projectId)
        {
            try
            {
                string eventType = action["eventType"]?.ToString();
                string eventLevel = action["eventLevel"]?.ToString();
                string description = action["description"]?.ToString();
                
                // 替换描述中的变量
                if (!string.IsNullOrEmpty(description))
                {
                    foreach (var key in dataPoint.Keys)
                    {
                        description = description.Replace($"{{{key}}}", dataPoint[key]?.ToString());
                    }
                }
                
                Logger.Info($"记录事件: 类型={eventType}, 级别={eventLevel}, 描述={description}");
                
                // TODO: 实现具体的事件记录逻辑
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.Error($"记录事件失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存告警数据到InfluxDB
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="rule">触发告警的规则</param>
        /// <param name="dataPoint">触发告警的数据点</param>
        /// <param name="conditionResults">条件评估结果</param>
        /// <returns>保存任务</returns>
        private async Task SaveAlarmToInfluxDBAsync(string projectId, RuleChainDto rule, Dictionary<string, object> dataPoint, List<string> conditionResults,string TenantAbbreviation)
        {
            try
            {
                
                // 获取租户简称，如果数据点中没有，则使用默认值
                string tenantAbbreviation = TenantAbbreviation;
                
                // 构建告警数据
                var alarmData = new Dictionary<string, object>
                {
                    ["projectId"] = projectId,
                    ["ruleId"] = rule.Id,
                    ["ruleName"] = rule.Name,
                    ["alarmLevel"] ="warning", // 默认为warning级别
                    ["alarmType"] = "scene_linkage",
                    ["message"] = $"触发场景联动规则: {rule.Name}",
                    ["conditionDetails"] = string.Join("; ", conditionResults),
                    ["notified"] = false,
                    ["acknowledged"] = false,
                    ["remark"] = "",
                    ["time"] = DateTime.Now
                };
                
                // 从数据点中提取关键信息
                if (dataPoint.TryGetValue("pointType", out object pointTypeObj))
                {
                    alarmData["pointType"] = pointTypeObj;
                }
                
                if (dataPoint.TryGetValue("pointId", out object pointIdObj))
                {
                    alarmData["pointId"] = pointIdObj;
                }
                
                // 解析规则条件
                if (!string.IsNullOrEmpty(rule.RuleContentJson))
                {
                    var ruleContent = JsonConvert.DeserializeObject<JObject>(rule.RuleContentJson);
                    if (ruleContent != null && ruleContent.ContainsKey("conditions"))
                    {
                        var conditions = ruleContent["conditions"] as JArray;
                        if (conditions != null && conditions.Any())
                        {
                            // 遍历所有条件
                            foreach (var condition in conditions)
                            {
                                string targetId = condition["targetId"]?.ToString();
                                string paramCode = condition["paramCode"]?.ToString();
                                string operatorType = condition["operator"]?.ToString();
                                string conditionValue = condition["value"]?.ToString();
                                
                                // 如果数据点ID与条件中的目标ID匹配
                                if (!string.IsNullOrEmpty(targetId) && 
                                    dataPoint.TryGetValue("pointId", out object pointId) && 
                                    pointId?.ToString() == targetId)
                                {
                                    // 添加目标ID到告警数据
                                    alarmData["targetId"] = targetId;
                                    
                                    // 如果参数编码存在且数据点中有对应的值
                                    if (!string.IsNullOrEmpty(paramCode) && 
                                        dataPoint.TryGetValue(paramCode, out object paramValue))
                                    {
                                        // 添加触发参数和值到告警数据
                                        alarmData[$"trigger_param"] = paramCode;
                                        alarmData[$"trigger_value"] = paramValue;
                                        alarmData[$"threshold_operator"] = operatorType;
                                        alarmData[$"threshold_value"] = conditionValue;
                                    }
                                }
                            }
                        }
                    }
                }
                
                // 保存到InfluxDB
                var measurementName = "alarms";
                var dataPoints = new List<Dictionary<string, object>> { alarmData };
                
                await _dataRepository.SaveDataPointsAsync(measurementName, tenantAbbreviation, dataPoints);
                
                Logger.Info($"成功保存告警数据到InfluxDB，规则: {rule.Name}, 项目: {projectId}");
            }
            catch (Exception ex)
            {
                Logger.Error("保存告警数据到InfluxDB失败", ex);
            }
        }
        /// <summary>
        /// 处理告警通知动作
        /// </summary>
        /// <param name="action">动作配置</param>
        /// <param name="rule">规则链</param>
        /// <param name="dataPoint">触发规则的数据点</param>
        /// <param name="projectId">项目ID</param>
        /// <returns>处理任务</returns>
        private async Task ProcessAlarmNotifyAction(JToken action, RuleChainDto rule, Dictionary<string, object> dataPoint, string projectId)
        {
            try
            {
                // 获取告警场景ID
                string alarmSceneId = action["alarmSceneId"]?.ToString();

                if (string.IsNullOrEmpty(alarmSceneId))
                {
                    Logger.Warn("告警场景ID为空，无法处理告警通知");
                    return;
                }

                Logger.Info($"处理告警通知动作，告警场景ID: {alarmSceneId}");

                // TODO: 根据告警场景ID查询告警配置信息
                // 例如：告警级别、通知方式、接收人等

                // TODO: 发送告警通知
                // 例如：短信、邮件、推送等
            }
            catch (Exception ex)
            {
                Logger.Error($"处理告警通知动作失败: {ex.Message}");
            }
        }
    }
}


