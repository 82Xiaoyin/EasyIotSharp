namespace EasyIotSharp.Core.Dto.Queue.Params
{
    /// <summary>
    /// 查询RabbitMQ服务器配置信息的入参类
    /// </summary>
    public class QueryRabbitServerInfoInput : PagingInput
    {
        /// <summary>
        /// 关键字（主机地址/用户名）
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int IsEnable { get; set; }
    }
}