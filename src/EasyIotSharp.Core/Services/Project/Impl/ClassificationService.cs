using EasyIotSharp.Core.Caching.Project;
using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Dto.Project;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Events.Project;
using EasyIotSharp.Core.Events.Tenant;
using EasyIotSharp.Core.Repositories.Project;
using EasyIotSharp.Core.Services.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Project.Impl
{
    public class ClassificationService:ServiceBase,IClassificationService
    {
        private readonly IProjectBaseRepository _projectBaseRepository;
        private readonly IClassificationRepository _classificationRepository;
        private readonly IClassificationCacheService _classificationCacheService;

        public ClassificationService(IClassificationRepository classificationRepository,
                                     IProjectBaseRepository projectBaseRepository,
                                     IClassificationCacheService classificationCacheService)
        {
            _projectBaseRepository = projectBaseRepository;
            _classificationRepository = classificationRepository;
            _classificationCacheService = classificationCacheService;
        }

        public async Task<ClassificationDto> GetClassification(string id)
        {
            var info = await _classificationRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            var output = info.MapTo<ClassificationDto>();
            if (output.IsNotNull())
            {
                var project = await _projectBaseRepository.GetByIdAsync(output.ProjectId);
                if (project.IsNotNull())
                {
                    output.ProjectName = project.Name;
                }
            }
            return output;
        }

        public async Task<PagedResultDto<ClassificationDto>> QueryClassification(QueryClassificationInput input)
        {
            if (input.ProjectId.IsNull()
                && input.IsPage.Equals(true)
                && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _classificationCacheService.QueryClassification(input, async () =>
                {
                    var query = await _classificationRepository.Query(ContextUser.TenantNumId, input.ProjectId, input.PageIndex, input.PageSize, input.IsPage);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<ClassificationDto>>();
                    var projects = await _projectBaseRepository.QueryByIds(list.Select(x => x.ProjectId).ToList());
                    foreach (var item in list)
                    {
                        var project = projects.FirstOrDefault(x => x.Id == item.ProjectId);
                        if (project.IsNotNull())
                        {
                            item.ProjectName = project.Name;
                        }
                    }
                    return new PagedResultDto<ClassificationDto>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _classificationRepository.Query(ContextUser.TenantNumId, input.ProjectId, input.PageIndex, input.PageSize, input.IsPage);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<ClassificationDto>>();
                var projects = await _projectBaseRepository.QueryByIds(list.Select(x => x.ProjectId).ToList());
                foreach (var item in list)
                {
                    var project = projects.FirstOrDefault(x => x.Id == item.ProjectId);
                    if (project.IsNotNull())
                    {
                        item.ProjectName = project.Name;
                    }
                }
                return new PagedResultDto<ClassificationDto>() { TotalCount = totalCount, Items = list };
            }
        }

        public async Task InsertClassification(InsertClassificationInput input)
        {
            var isExistName = await _classificationRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Name == input.Name && x.ProjectId == input.ProjectId && x.IsDelete == false);
            if (isExistName.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "分类名称重复");
            }
            var model = new Classification();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.TenantNumId = ContextUser.TenantNumId;
            model.Name = input.Name;
            model.ProjectId = input.ProjectId;
            model.Sort = input.Sort;
            model.IsDelete = false;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _classificationRepository.InsertAsync(model);

            //清除缓存
            await EventBus.TriggerAsync(new ClassificationEventData() { });
        }

        public async Task UpdateClassification(UpdateClassificationInput input)
        {
            var info = await _classificationRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR,"分类不存在");
            }
            var isExistName = await _classificationRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Name == input.Name && x.ProjectId == info.ProjectId && x.IsDelete == false);
            if (isExistName.IsNotNull() && isExistName.Id != input.Id)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "分类名称重复");
            }
            info.Name = input.Name;
            info.Sort = input.Sort;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _classificationRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new ClassificationEventData() { });
        }

        public async Task DeleteClassification(DeleteClassificationInput input)
        {
            var info = await _classificationRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.IsNotNull())
            {
                await _classificationRepository.DeleteByIdAsync(info.Id);
            }

            //清除缓存
            await EventBus.TriggerAsync(new ClassificationEventData() { });
        }
    }
}
