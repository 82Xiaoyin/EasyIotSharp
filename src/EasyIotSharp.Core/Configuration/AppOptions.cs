using Microsoft.Extensions.Configuration;

namespace EasyIotSharp.Core.Configuration
{
    /// <summary>
    /// 【配置项】应用下所有配置的聚合
    /// </summary>
    public class AppOptions
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 主机地址+端口
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 最小工作线程
        /// 16 = 不设置
        /// https://stackexchange.github.io/StackExchange.Redis/Timeouts
        /// 当大量并发进入服务器时，线程池立即创建线程来达到这个最小值，超过最小值后线程池创建新的线程速率是比较慢的，这时新的线程来不及创建补充从而造成阻塞排队。
        /// 16 核 16G 最大工作线程 = 32767
        /// </summary>
        public int SetMinThreads { get; internal set; }

        /// <summary>
        /// 存储配置
        /// </summary>
        public StorageOptions StorageOptions { get; set; }

        /// <summary>
        /// 缓存配置
        /// </summary>
        public CachingOptions CachingOptions { get; set; }

        /// <summary>
        /// DMS api 地址相关
        /// </summary>
        public APIServiceOptions APIServiceOptions { get; internal set; }

        /// <summary>
        /// minio配置
        /// </summary>
        public MinIOOptions MinIOOptions { get; set; }


        /// <summary>
        /// MQTT配置
        /// </summary>
        public MqttOptions MqttOptions { get; set; }

        /// <summary>
        /// es 分页读写相关
        /// </summary>
        public ESPaggingOptions ESPaggingOptions { get; internal set; }

        /// <summary>
        /// 消息队列配置
        /// </summary>
        public QueueOptions QueueOptions { get; set; }

        /// <summary>
        /// 企业微信通知发送配置
        /// </summary>
        public CorpWeixinOptions CorpWeixinOptions { get; internal set; }

        /// <summary>
        /// 初始化读取
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static AppOptions ReadFromConfiguration(IConfiguration config)
        {
            AppOptions options = new AppOptions();
            options.Name = config.GetValue<string>(nameof(Name));
            options.Host = config.GetValue<string>(nameof(Host));
            options.StorageOptions = StorageOptions.ReadFromConfiguration(config);
            options.APIServiceOptions = APIServiceOptions.ReadFromConfiguration(config);
            options.CachingOptions = CachingOptions.ReadFromConfiguration(config);
            options.MinIOOptions = MinIOOptions.ReadFromConfiguration(config);
            options.MqttOptions = MqttOptions.ReadFromConfiguration(config);
            options.QueueOptions = QueueOptions.ReadFromConfiguration(config);
            options.ESPaggingOptions = ESPaggingOptions.ReadFromConfiguration(config);
            options.QueueOptions = QueueOptions.ReadFromConfiguration(config);
            options.CorpWeixinOptions = CorpWeixinOptions.ReadFromConfiguration(config);
            return options;
        }
    }
}