using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Dto.Project;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Repositories.Project;
using EasyIotSharp.Core.Dto.Tenant;
using UPrime.AutoMapper;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Domain.Queue;

namespace EasyIotSharp.Core.Services.Project.Impl
{
    public class ProjectBaseService : ServiceBase, IProjectBaseService
    {
        private readonly IProjectBaseRepository _projectBaseRepository;

        public ProjectBaseService(IProjectBaseRepository projectBaseRepository)
        {
            _projectBaseRepository = projectBaseRepository;
        }

        public async Task<ProjectBaseDto> GetProjectBase(string id)
        {
            var info = await _projectBaseRepository.QueryByProjectBaseFirst(id);
            return info.MapTo<ProjectBaseDto>();
        }

        public async Task<PagedResultDto<ProjectBaseDto>> QueryProjectBase(QueryProjectBaseInput input)
        {
            var query = await _projectBaseRepository.Query(ContextUser.TenantNumId, input.Keyword, input.State, input.CreateStartTime, input.CreateEndTime, input.PageIndex, input.PageSize);
            int totalCount = query.totalCount;
            var list = query.items.MapTo<List<ProjectBaseDto>>();

            return new PagedResultDto<ProjectBaseDto>() { TotalCount = totalCount, Items = list };
        }

        public async Task InsertProjectBase(InsertProjectBaseInput input)
        {
            var isExistName = await _projectBaseRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Name == input.Name && x.IsDelete == false);
            if (isExistName.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "项目名称重复");
            }
            var model = new ProjectBase();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.TenantNumId = ContextUser.TenantNumId;
            model.Name = input.Name;
            model.Longitude = input.Longitude;
            model.latitude = input.latitude;
            model.State = 0;
            model.Address = input.Address;
            model.Remark = input.Remark;
            model.IsDelete = false;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _projectBaseRepository.InsertAsync(model);

            var rabbitProject = new RabbitProject();
            rabbitProject.Id = Guid.NewGuid().ToString().Replace("-", "");
            rabbitProject.RabbitServerInfoId = input.RabbitServerInfoId;
            rabbitProject.ProjectId = model.Id;
            await _projectBaseRepository.AddRabbitProject(rabbitProject);
        }

        public async Task UpdateProjectBase(UpdateProjectBaseInput input)
        {
            var info = await _projectBaseRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            var isExistName = await _projectBaseRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Name == input.Name && x.IsDelete == false);
            if (isExistName.IsNotNull() && isExistName.Id != input.Id)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "项目名称重复");
            }
            info.Name = input.Name;
            info.Longitude = input.Longitude;
            info.latitude = input.latitude;
            info.Address = input.Address;
            info.Remark = input.Remark;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            info.State = input.State == true ? 0 : 1;
            await _projectBaseRepository.UpdateAsync(info);

            var rabbitProject = await _projectBaseRepository.QueryRabbitProject(info.Id);
            if (rabbitProject != null)
            {
                rabbitProject.RabbitServerInfoId = input.RabbitServerInfoId;
                rabbitProject.ProjectId = info.Id;
                await _projectBaseRepository.UpdateRabbitProject(rabbitProject);
            }
            else
            {
                var addRabbitProject = new RabbitProject();
                addRabbitProject.Id = Guid.NewGuid().ToString().Replace("-", "");
                addRabbitProject.RabbitServerInfoId = input.RabbitServerInfoId;
                addRabbitProject.ProjectId = info.Id;
                await _projectBaseRepository.AddRabbitProject(addRabbitProject);
            }
        }

        public async Task UpdateStateProjectBase(UpdateStateProjectBaseInput input)
        {
            var info = await _projectBaseRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.State != input.State)
            {
                info.State = input.State;
                info.UpdatedAt = DateTime.Now;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                await _projectBaseRepository.UpdateAsync(info);
            }
        }

        public async Task DeleteProjectBase(DeleteProjectBaseInput input)
        {
            var info = await _projectBaseRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.IsDelete != input.IsDelete)
            {
                info.IsDelete = input.IsDelete;
                info.UpdatedAt = DateTime.Now;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                await _projectBaseRepository.UpdateAsync(info);
            }

            var rabbitProject = await _projectBaseRepository.QueryRabbitProject(info.Id);
            if (rabbitProject != null)
            {
                rabbitProject.IsDelete = input.IsDelete;
                rabbitProject.UpdatedAt = DateTime.Now;
                rabbitProject.OperatorId = ContextUser.UserId;
                rabbitProject.OperatorName = ContextUser.UserName;
                await _projectBaseRepository.UpdateRabbitProject(rabbitProject);
            }
            ;
        }
    }
}
