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
using EasyIotSharp.Core.Caching.Project;
using EasyIotSharp.Core.Caching.Project.Impl;
using EasyIotSharp.Core.Events.Tenant;
using EasyIotSharp.Core.Events.Project;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.IO.Compression;
using EasyIotSharp.Core.Services.IO;

namespace EasyIotSharp.Core.Services.Project.Impl
{
    public class ProjectBaseService : ServiceBase, IProjectBaseService
    {
        private readonly ITempFolderService _tempFolderService;
        private readonly IMinIOFileService _minIOFileService;

        private readonly IProjectBaseRepository _projectBaseRepository;
        private readonly IProjectBaseCacheService _projectBaseCacheService;

        public ProjectBaseService(IProjectBaseRepository projectBaseRepository,
                                  IProjectBaseCacheService projectBaseCacheService,
                                  ITempFolderService tempFolderService,
                                  IMinIOFileService minIOFileService)
        {
            _tempFolderService = tempFolderService;
            _minIOFileService = minIOFileService;

            _projectBaseRepository = projectBaseRepository;
            _projectBaseCacheService = projectBaseCacheService;
        }

        public async Task<ProjectBaseDto> GetProjectBase(string id)
        {
            var info = await _projectBaseRepository.QueryByProjectBaseFirst(id);
            return info.MapTo<ProjectBaseDto>();
        }

        public async Task<PagedResultDto<ProjectBaseDto>> QueryProjectBase(QueryProjectBaseInput input)
        {
            if (string.IsNullOrEmpty(input.Keyword)
                && input.State.Equals(-1)
                && input.CreateEndTime.IsNull()
                && input.CreateStartTime.IsNull()
                && input.IsPage.Equals(true)
                && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _projectBaseCacheService.QueryProjectBase(input, async () =>
                {
                    var query = await _projectBaseRepository.Query(ContextUser.TenantNumId, input.Keyword, input.State, input.CreateStartTime, input.CreateEndTime, input.PageIndex, input.PageSize);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<ProjectBaseDto>>();

                    return new PagedResultDto<ProjectBaseDto>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _projectBaseRepository.Query(ContextUser.TenantNumId, input.Keyword, input.State, input.CreateStartTime, input.CreateEndTime, input.PageIndex, input.PageSize);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<ProjectBaseDto>>();

                return new PagedResultDto<ProjectBaseDto>() { TotalCount = totalCount, Items = list };
            }
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

            //清除缓存
            await EventBus.TriggerAsync(new ProjectBaseEventData() { });
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
            info.State = input.State == true ? 1 : 0;
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

            //清除缓存
            await EventBus.TriggerAsync(new ProjectBaseEventData() { });
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

            //清除缓存
            await EventBus.TriggerAsync(new ProjectBaseEventData() { });
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
            };

            //清除缓存
            await EventBus.TriggerAsync(new ProjectBaseEventData() { });
        }

        public async Task<string> UploadProjectUnity(IFormFile formFile)
        {
            var fileExtension = Path.GetExtension(formFile.FileName).ToLower();
            if (fileExtension != ".zip")
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "请上传.zip后缀的文件");
            }
            //获取临时压缩包文件夹目录
            var zipTempPath = _tempFolderService.GetZipFolderPath("");

            // 保存上传的文件
            var uploadedFilePath = Path.Combine(zipTempPath, formFile.FileName);
            using (var stream = new FileStream(uploadedFilePath, FileMode.Create))
            {
                await formFile.CopyToAsync(stream);
            }
            // 解压文件
            var extractPath = Path.Combine(zipTempPath, formFile.FileName.Split(".")[0]);
            Directory.CreateDirectory(extractPath);
            ZipFile.ExtractToDirectory(uploadedFilePath, extractPath);

            // 处理需要上传文件拼接地址
            var processedFiles = ProcessExtractedFiles(extractPath);
            Dictionary<string, string> uploadFiles = new Dictionary<string, string>();
            foreach (var file in processedFiles)
            {
                uploadFiles.Add(file.Key, formFile.FileName.Split(".")[0] + "/" + file.Value);
            }
            string defaultUrl = "";
            foreach (var item in uploadFiles)
            {
                string url=await _minIOFileService.UploadAsync(item.Value, item.Key);

                if (item.Value.Contains("index.html"))
                {
                    defaultUrl = url;
                }
            }
            #region 删除临时文件夹

            _tempFolderService.DeleteFiles(zipTempPath);

            #endregion 删除临时文件夹

            return defaultUrl;
        }

        public static Dictionary<string, string> ProcessExtractedFiles(string rootPath)
        {
            var processedFiles = new Dictionary<string, string>();
            ProcessDirectory(rootPath, rootPath, processedFiles);
            return processedFiles;
        }

        private static void ProcessDirectory(string currentDir, string rootPath, Dictionary<string, string> processedFiles)
        {
            try
            {
                // 处理当前目录下的所有文件
                foreach (string filePath in Directory.GetFiles(currentDir))
                {
                    // 获取相对于根目录的路径
                    string relativePath = GetRelativePath(filePath, rootPath);

                    // 获取拼接后的文件名（用下划线代替路径分隔符）
                    string prefixedFileName = relativePath.Replace(Path.DirectorySeparatorChar, '/');

                    // 添加到结果字典
                    processedFiles.Add(filePath, prefixedFileName);
                }

                // 递归处理子目录
                foreach (string directory in Directory.GetDirectories(currentDir))
                {
                    ProcessDirectory(directory, rootPath, processedFiles);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing directory {currentDir}: {ex.Message}");
            }
        }

        private static string GetRelativePath(string fullPath, string rootPath)
        {
            // 确保根路径以目录分隔符结尾
            if (!rootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                rootPath += Path.DirectorySeparatorChar;
            }

            Uri fullUri = new Uri(fullPath);
            Uri rootUri = new Uri(rootPath);

            // 获取相对路径并移除开头的目录分隔符
            string relativePath = Uri.UnescapeDataString(rootUri.MakeRelativeUri(fullUri).ToString())
                .Replace('/', Path.DirectorySeparatorChar);

            return relativePath;
        }
    }
}
