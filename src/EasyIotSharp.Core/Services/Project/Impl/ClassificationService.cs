﻿using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Dto.Project;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Repositories.Project;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Project.Impl
{
    public class ClassificationService:ServiceBase,IClassificationService
    {
        private readonly IClassificationRepository _classificationRepository;

        public ClassificationService(IClassificationRepository classificationRepository)
        {
            _classificationRepository = classificationRepository;
        }

        public async Task<ClassificationDto> GetClassification(string id)
        {
            var info = await _classificationRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            return info.MapTo<ClassificationDto>();
        }

        public async Task<PagedResultDto<ClassificationDto>> QueryClassification(QueryClassificationInput input)
        {
            var query = await _classificationRepository.Query(ContextUser.TenantNumId, input.ProjectId, input.PageIndex, input.PageSize, input.isPage);
            int totalCount = query.totalCount;
            var list = query.items.MapTo<List<ClassificationDto>>();

            return new PagedResultDto<ClassificationDto>() { TotalCount = totalCount, Items = list };
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
        }

        public async Task DeleteClassification(DeleteClassificationInput input)
        {
            var info = await _classificationRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.IsNotNull())
            {
                await _classificationRepository.DeleteByIdAsync(info.Id);
            }
        }
    }
}
