
namespace EasyIotSharp.Core.Dto.Project.Params
{
    /// <summary>
    /// 通过id修改一条项目信息
    /// </summary>
    public class UpdateProjectBaseInput
    {
        /// <summary>
        /// 项目id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 服务器配置表Id
        /// </summary>
        public string RabbitServerInfoId { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public string Longitude { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public string latitude { get; set; }

        /// <summary>
        /// 项目地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 资源Id
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// 项目描述
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; }
    }
}
