using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using EasyIotSharp.DataProcessor.Util.Encrypotion;

namespace EasyIotSharp.DataProcessor.Model.AnalysisDTO
{
    /// <summary>
    /// 传感器数据类型
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// 低频数据
        /// </summary>
        LowFrequency = 1,

        /// <summary>
        /// 高频数据
        /// </summary>
        HighFrequency = 2
    }

    /// <summary>
    /// 传感器数据基类
    /// </summary>
    public class SensorDataBase
    {
        /// <summary>
        /// 租户简称
        /// </summary>
        public string TenantAbbreviation { get; set; }
        /// <summary>
        /// 项目ID
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// 时间戳（17位字符串）
        /// </summary>
        public DateTime Time { get; set; }


        /// <summary>
        /// 测点类型
        /// </summary>
        public string PointType { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// 将对象加密为字符串
        /// </summary>
        public string ToEncryptedString(string key = "EasyIotSharp2024")
        {
            string jsonString = JsonConvert.SerializeObject(this);
            return AESHelper.Encrypt(jsonString, key);
        }

        /// <summary>
        /// 从加密字符串解密为对象
        /// </summary>
        public static T FromEncryptedString<T>(string encryptedString, string key = "EasyIotSharp2024") where T : SensorDataBase
        {
            string jsonString = AESHelper.Decrypt(encryptedString, key);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }

    /// <summary>
    /// 低频数据
    /// </summary>
    public class LowFrequencyData : SensorDataBase
    {
        /// <summary>
        /// 测点数据列表
        /// </summary>
        public List<PointData> Points { get; set; } = new List<PointData>();
    }

    /// <summary>
    /// 高频数据
    /// </summary>
    public class HighFrequencyData : SensorDataBase
    {
        /// <summary>
        /// 采集周期（ms）
        /// </summary>
        public int SamplingPeriod { get; set; }

        /// <summary>
        /// 测点数据列表
        /// </summary>
        public List<HighFrequencyPointData> Points { get; set; } = new List<HighFrequencyPointData>();
    }
    public class NamedValue
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
    }
    /// <summary>
    /// 测点数据
    /// </summary>
    public class PointData
    {
        /// <summary>
        /// 测点ID
        /// </summary>
        public string PointId { get; set; }

        /// <summary>
        /// 指标数量
        /// </summary>
        public int IndicatorCount { get; set; }

        /// <summary>
        /// 数据值列表
        /// </summary>
        public List<NamedValue> Values { get; set; } = new List<NamedValue>();
    }

    /// <summary>
    /// 高频测点数据
    /// </summary>
    public class HighFrequencyPointData
    {
        /// <summary>
        /// 测点ID
        /// </summary>
        public string PointId { get; set; }

        /// <summary>
        /// 指标数量
        /// </summary>
        public int IndicatorCount { get; set; }

        /// <summary>
        /// 采样数据组
        /// </summary>
        public List<List<double>> SampleGroups { get; set; } = new List<List<double>>();
    }

    /// <summary>
    /// 传感器数据工厂
    /// </summary>
    public static class SensorDataFactory
    {
        /// <summary>
        /// 创建传感器数据对象
        /// </summary>
        public static T CreateSensorData<T>(string projectId, DateTime timestamp) where T : SensorDataBase, new()
        {
            return new T
            {
                ProjectId = projectId,
                Time = timestamp,
                DataType = typeof(T) == typeof(LowFrequencyData) ? DataType.LowFrequency : DataType.HighFrequency
            };
        }
    }
}
