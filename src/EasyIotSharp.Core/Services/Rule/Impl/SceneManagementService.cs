using System;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Dto;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Repositories.Rule;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Rule.Impl
{
    /// <summary>
    /// 场景管理服务
    /// </summary>
    public class SceneManagementService : ServiceBase, ISceneManagementService
    {
        private readonly ISceneManagementRepository _sceneManagementRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SceneManagementService(ISceneManagementRepository sceneManagementRepository)
        {
            _sceneManagementRepository = sceneManagementRepository;
        }

        /// <summary>
        /// 查询场景列表
        /// </summary>
        public async Task<PagedResultDto<SceneManagementDto>> QuerySceneManagement(SceneManagementInput input)
        {
            var query = await _sceneManagementRepository.Query(
                input.Keyword,
                input.PageIndex,
                input.PageSize);

            return new PagedResultDto<SceneManagementDto>()
            {
                TotalCount = query.totalCount,
                Items = query.items
            };
        }

        /// <summary>
        /// 新增场景
        /// </summary>
        public async Task InsertSceneManagement(InsertSceneManagement input)
        {
            var isExist = await _sceneManagementRepository.FirstOrDefaultAsync(x =>
                x.SceneName == input.SceneName &&
                x.IsDelete == false);

            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "场景名称重复");
            }

            var model = new SceneManagement
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                SceneName = input.SceneName,
                ActionJSON = input.ActionJSON,
                SceneRemark = input.SceneRemark,
                CreationTime = DateTime.Now,
                UpdatedAt = DateTime.Now,
                OperatorId = ContextUser.UserId,
                OperatorName = ContextUser.UserName
            };

            await _sceneManagementRepository.InsertAsync(model);
        }

        /// <summary>
        /// 修改场景
        /// </summary>
        public async Task UpdateSceneManagement(InsertSceneManagement input)
        {
            var info = await _sceneManagementRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }

            var isExist = await _sceneManagementRepository.FirstOrDefaultAsync(x =>
                x.Id != input.Id &&
                x.SceneName == input.SceneName &&
                x.IsDelete == false);

            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "场景名称重复");
            }

            info.SceneName = input.SceneName;
            info.ActionJSON = input.ActionJSON;
            info.SceneRemark = input.SceneRemark;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;

            await _sceneManagementRepository.UpdateAsync(info);
        }

        /// <summary>
        /// 删除场景
        /// </summary>
        public async Task DeleteSceneManagement(DeleteInput input)
        {
            var info = await _sceneManagementRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }

            info.IsDelete = true;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;

            await _sceneManagementRepository.UpdateAsync(info);
        }
    }
}