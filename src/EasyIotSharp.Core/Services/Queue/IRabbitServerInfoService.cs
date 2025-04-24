using EasyIotSharp.Core.Dto.Queue;
using EasyIotSharp.Core.Dto.Queue.Params;
using System.Collections.Generic;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Queue
{
    /// <summary>
    /// RabbitMQ服务器配置信息服务接口
    /// </summary>
    public interface IRabbitServerInfoService
    {
        /// <summary>
        /// 通过ID获取一条RabbitMQ服务器配置信息
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        Task<RabbitServerInfoDto> GetRabbitServerInfo(string id);

        /// <summary>
        /// 添加一条RabbitMQ服务器配置信息
        /// </summary>
        /// <param name="input">输入参数</param>
        /// <returns></returns>
        Task  InsertRabbitServerInfo(InsertRabbitServerInfoInput input);

        /// <summary>
        /// 修改一条RabbitMQ服务器配置信息
        /// </summary>
        /// <param name="input">输入参数</param>
        /// <returns></returns>
        Task  UpdateRabbitServerInfo(UpdateRabbitServerInfoInput input);

        /// <summary>
        /// 删除一条RabbitMQ服务器配置信息
        /// </summary>
        /// <param name="input">输入参数</param>
        /// <returns></returns>
        Task  DeleteRabbitServerInfo(DeleteRabbitServerInfoInput input);

        /// <summary>
        /// 分页查询RabbitMQ服务器配置信息列表
        /// </summary>
        /// <param name="input">输入参数</param>
        /// <returns></returns>
        Task<PagedResultDto<RabbitServerInfoDto>> QueryRabbitServerInfo(QueryRabbitServerInfoInput input);

        /// <summary>
        /// 获取rabbit配置信息列表
        /// </summary>
        /// <returns></returns>
        List<RabbitServerInfoDto> GetRabbitProject();
    }
}