using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Domain.Queue;
using EasyIotSharp.Core.Dto.Project;
using EasyIotSharp.Core.Repositories.Mysql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Project
{
    public interface IProjectBaseRepository : IMySqlRepositoryBase<ProjectBase, string>
    {
        /// <summary>
        /// 通过条件分页查询项目列表
        /// </summary>
        /// <param name="tenantNumId">租户NumId</param>
        /// <param name="keyword">项目名称/备注</param>
        /// <param name="state">状态 -1=全部  0=初始化状态  1=正在运行状态</param>
        /// <param name="createStartTime">创建开始时间</param>
        /// <param name="createEndTime">创建结束时间</param>
        /// <param name="pageIndex">起始页</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <returns></returns>
        Task<(int totalCount, List<ProjectBaseDto> items)> Query(int tenantNumId,
                                                              string keyword,
                                                              bool? state,
                                                              DateTime? createStartTime,
                                                              DateTime? createEndTime,
                                                              int pageIndex,
                                                              int pageSize);

        /// <summary>
        /// 根据ID拿到一条
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ProjectBaseDto> QueryByProjectBaseFirst(string id);

        /// <summary>
        /// 根据ID集合查询项目列表
        /// </summary>
        /// <param name="ids">ID集合</param>
        /// <returns></returns>
        Task<List<ProjectBase>> QueryByIds(List<string> ids);

        /// <summary>
        /// 根据ID拿到一条
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<RabbitProject> QueryRabbitProject(string projectId);

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="rabbitProject"></param>
        /// <returns></returns>
        Task AddRabbitProject(RabbitProject rabbitProject);

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="rabbitProject"></param>
        Task UpdateRabbitProject(RabbitProject rabbitProject);
    }
}
