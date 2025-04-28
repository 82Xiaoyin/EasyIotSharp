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
using System.Threading;
using EasyIotSharp.Core.Dto.Enum;

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
            model.ResourceId = input.ResourceId;
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
            info.ResourceId = input.ResourceId;
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
            }
            ;

            //清除缓存
            await EventBus.TriggerAsync(new ProjectBaseEventData() { });
        }

        /// <summary>
        /// 上传Unity项目压缩包并部署到MinIO
        /// </summary>
        /// <param name="formFile">上传的Unity项目压缩文件</param>
        /// <returns>部署后的访问URL</returns>
        public async Task<string> UploadProjectUnity(string name, ResourceEnums type, IFormFile formFile)
        {
            // 临时文件和目录路径
            string uniqueFolderName = type.ToString() + "/" + Guid.NewGuid().ToString();
            string zipTempPath = null;
            string extractPath = null;
            string uploadedFilePath = null;

            try
            {
                // 1. 验证文件格式
                var fileExtension = Path.GetExtension(formFile.FileName).ToLower();
                if (fileExtension != ".zip")
                {
                    throw new BizException(BizError.BIND_EXCEPTION_ERROR, "请上传.zip后缀的文件");
                }

                // 2. 准备临时目录和文件
                zipTempPath = _tempFolderService.GetZipFolderPath("");
                extractPath = Path.Combine(zipTempPath, uniqueFolderName);
                string uniqueFileName = $"{Path.GetFileNameWithoutExtension(formFile.FileName)}_{Guid.NewGuid()}.zip";
                uploadedFilePath = Path.Combine(zipTempPath, uniqueFileName);

                // 确保解压目录存在且为空
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                Directory.CreateDirectory(extractPath);
                Logger.Debug($"已准备解压目录: {extractPath}");

                // 3. 保存上传的文件
                using (var stream = new FileStream(uploadedFilePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }
                Logger.Debug($"已保存上传文件到: {uploadedFilePath}");

                // 4. 解压文件
                await Task.Run(() =>
                {
                    using (ZipArchive archive = ZipFile.OpenRead(uploadedFilePath))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string destinationPath = Path.Combine(extractPath, entry.FullName);
                            string destinationDirectory = Path.GetDirectoryName(destinationPath);

                            // 创建目标目录（如果不存在）
                            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                            {
                                Directory.CreateDirectory(destinationDirectory);
                            }

                            // 跳过目录条目
                            if (!string.IsNullOrEmpty(entry.Name))
                            {
                                // 解压文件，处理文件锁定情况
                                int retryCount = 0;
                                int maxRetries = 3;

                                while (true)
                                {
                                    try
                                    {
                                        entry.ExtractToFile(destinationPath, true);
                                        break; // 成功则退出循环
                                    }
                                    catch (IOException ex) when (retryCount < maxRetries &&
                                                                (ex.Message.Contains("because it is being used by another process") ||
                                                                 ex.Message.Contains("already exists")))
                                    {
                                        retryCount++;
                                        Thread.Sleep(1000);
                                        Logger.Debug($"解压文件 {entry.FullName} 重试 ({retryCount}/{maxRetries})");
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error($"解压文件 {entry.FullName} 失败: {ex.Message}");
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                });
                Logger.Debug($"已解压文件 {uploadedFilePath} 到 {extractPath}");

                // 5. 处理解压后的文件并上传到MinIO
                // 处理需要上传文件拼接地址
                var processedFiles = new Dictionary<string, string>();
                ProcessDirectory(extractPath, extractPath, processedFiles);

                Dictionary<string, string> uploadFiles = new Dictionary<string, string>();
                foreach (var file in processedFiles)
                {
                    uploadFiles.Add(file.Key, uniqueFolderName + "/" + file.Value);
                }

                // 6. 上传文件到MinIO
                string defaultUrl = "";
                int totalFiles = uploadFiles.Count;
                int processedCount = 0;

                foreach (var item in uploadFiles)
                {
                    await _minIOFileService.UploadAsync(ContextUser?.TenantAbbreviation.ToLower(), item.Value, item.Key);
                    string url = await _minIOFileService.GetFileUrlAsync(ContextUser?.TenantAbbreviation.ToLower(), item.Value);
                    processedCount++;

                    // 记录上传进度
                    Logger.Debug($"已上传文件 {processedCount}/{totalFiles}/{ContextUser?.TenantAbbreviation.ToLower()}: {item.Value}");

                    // 如果是index.html文件，保存其URL作为返回值
                    if (item.Value.Contains("index.html"))
                    {
                        defaultUrl = url;
                        Logger.Debug($"找到主页文件: {item.Value}, URL: {url}");
                    }
                }

                Logger.Info($"Unity项目上传完成，共 {totalFiles} 个文件，主页URL: {defaultUrl}");
                return defaultUrl;
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                Logger.Error($"上传Unity项目失败: {ex.Message}", ex);
                return $"上传Unity项目失败: {ex.Message}";
            }
            finally
            {
                // 7. 清理临时文件
                if (!string.IsNullOrEmpty(zipTempPath) && Directory.Exists(zipTempPath))
                {
                    try
                    {
                        // 等待一段时间，确保所有文件操作完成
                        await Task.Delay(500);
                        _tempFolderService.DeleteFiles(zipTempPath);
                        Logger.Debug($"已清理临时文件: {zipTempPath}");
                    }
                    catch (Exception cleanupEx)
                    {
                        // 记录清理错误但不抛出
                        Logger.Warn($"清理临时文件时出错: {cleanupEx.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 递归处理目录中的文件
        /// </summary>
        /// <param name="currentDir">当前处理的目录</param>
        /// <param name="rootPath">解压根目录</param>
        /// <param name="processedFiles">结果字典</param>
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
                Console.WriteLine($"处理目录 {currentDir} 时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取文件相对于根目录的路径
        /// </summary>
        /// <param name="fullPath">文件完整路径</param>
        /// <param name="rootPath">根目录路径</param>
        /// <returns>相对路径</returns>
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
