using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.File;
using System.Threading.Tasks;
using EasyIotSharp.Core.Repositories.Files;
using EasyIotSharp.Core.Repositories.Project;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Dto.File.Params;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using System.IO;
using EasyIotSharp.Core.Services.IO;
using System.Threading;
using System.Linq;
using EasyIotSharp.Core.Dto.Enum;
using EasyIotSharp.Core.Dto;
using Mysqlx.Crud;

namespace EasyIotSharp.Core.Services.Files.Impl
{
    public class ResourceService : ServiceBase, IResourceService
    {
        private readonly ITempFolderService _tempFolderService;
        private readonly IMinIOFileService _minIOFileService;
        private readonly IResourceRepository _resourceRepository;
        public ResourceService(IResourceRepository resourceRepository,
                               ITempFolderService tempFolderService,
                               IMinIOFileService minIOFileService)
        {
            _resourceRepository = resourceRepository;
            _tempFolderService = tempFolderService;
            _minIOFileService = minIOFileService;
        }
        /// <summary>
        /// 查询资源列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResultDto<ResourceDto>> QueryResources(ResourceInput input)
        {
            // 调用仓储层的查询方法
            var list = await _resourceRepository.Query(
                input.State,
                input.ResourceType,
                input.PageIndex,
                input.PageSize,
                input.IsPage);

            // 返回分页结果
            return new PagedResultDto<ResourceDto>
            {
                TotalCount = list.totalCount,
                Items = list.items
            };
        }

        /// <summary>
        /// 资源新增
        /// </summary>
        /// <param name="insert">上传资源参数</param>
        /// <returns>默认访问URL</returns>
        public async Task<string> UploadResponseInsert(ResourceInsert insert)
        {
            var url = await UploadResponse(insert.Name, insert.ResourceType, insert.FormFile);

            var resource = new Domain.Files.Resource
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                TenantId = ContextUser?.TenantId ?? Guid.NewGuid().ToString("N"),
                Name = insert.Name,
                Type = (int)insert.ResourceType,
                Url = url,
                State = true,
                Remark = insert.Remark ?? "",
                CreationTime = DateTime.Now,
                OperatorId = ContextUser?.UserId ?? "",
                OperatorName = ContextUser?.UserName ?? ""
            };

            // 保存到数据库
            await _resourceRepository.InsertAsync(resource);

            return resource.Url;
        }
        /// <summary>
        /// 修改资源信息
        /// </summary>
        /// <param name="input">资源信息</param>
        /// <returns>是否成功</returns>
        public async Task<string> UpdateResource(UpdateResourceInput input)
        {
            // 获取资源信息
            var resource = await _resourceRepository.GetByIdAsync(input.Id);
            if (resource == null)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "资源不存在");
            }
            try
            {
                if (input.FormFile != null)
                {
                    if (!string.IsNullOrEmpty(resource.Url))
                    {
                        // 从URL中提取对象名称
                        var uri = new Uri(resource.Url);
                        var pathSegments = uri.AbsolutePath.Split('/').Skip(2).ToArray(); // 跳过第一个空段

                        if (pathSegments.Length >= 2)
                        {
                            string bucketName = ContextUser?.TenantAbbreviation.ToLower() ?? "cs0001";
                            string objectName = string.Join("/", pathSegments);

                            // 删除MinIO中的文件
                            await _minIOFileService.DeleteFileAsync(bucketName, objectName);
                        }
                    }
                    resource.Url = await UploadResponse(input.Name, (ResourceEnums)input.Type, input.FormFile);
                }

                // 更新资源信息
                resource.Name = input.Name;
                resource.Remark = input.Remark;
                resource.State = input.State;
                resource.UpdatedAt = DateTime.Now;
                resource.OperatorId = ContextUser?.UserId;
                resource.OperatorName = ContextUser?.UserName;

                // 保存到数据库
                await _resourceRepository.UpdateAsync(resource);
            }
            catch (Exception ex)
            {
                Logger.Error($"修改资源失败: {ex.Message}", ex);
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, $"修改资源失败: {ex.Message}");
            }

            // 保存到数据库
            await _resourceRepository.UpdateAsync(resource);

            return resource.Url;
        }

        /// <summary>
        /// 删除资源
        /// </summary>
        /// <param name="id">资源ID</param>
        /// <returns>是否成功</returns>
        public async Task<string> DeleteResource(DeleteInput input)
        {
            // 获取资源信息
            var resource = await _resourceRepository.GetByIdAsync(input.Id);
            if (resource == null)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "资源不存在");
            }

            try
            {
                // 从MinIO中删除文件
                if (!string.IsNullOrEmpty(resource.Url))
                {
                    // 从URL中提取对象名称
                    var uri = new Uri(resource.Url);
                    var pathSegments = uri.AbsolutePath.Split('/').Skip(2).ToArray(); // 跳过第一个空段

                    if (pathSegments.Length >= 2)
                    {
                        string bucketName = ContextUser?.TenantAbbreviation.ToLower() ?? "cs0001";
                        string objectName = string.Join("/", pathSegments);

                        // 删除MinIO中的文件
                        await _minIOFileService.DeleteFileAsync(bucketName, objectName);
                    }
                }

                // 从数据库中删除记录
                await _resourceRepository.DeleteAsync(resource);

                return resource.Url;
            }
            catch (Exception ex)
            {
                Logger.Error($"删除资源失败: {ex.Message}", ex);
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, $"删除资源失败: {ex.Message}");
            }
        }


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="formFile"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
        public async Task<string> UploadResponse(string name, ResourceEnums type, IFormFile formFile)
        {
            if (formFile == null)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "上传文件不能为空");
            }

            var fileExtension = Path.GetExtension(formFile.FileName).ToLower();
            if (fileExtension != ".zip")
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "请上传.zip后缀的文件");
            }
            // 生成唯一标识符，用于文件夹和文件名
            string uniqueId = Guid.NewGuid().ToString();
            string uniqueFolderName = type.ToString() + "/" + uniqueId;

            // 获取临时压缩包文件夹目录
            var zipTempPath = _tempFolderService.GetZipFolderPath("");
            var extractPath = Path.Combine(zipTempPath, uniqueFolderName);
            var uploadedFilePath = Path.Combine(zipTempPath, $"{Path.GetFileNameWithoutExtension(formFile.FileName)}_{uniqueId}.zip");

            try
            {
                // 保存上传的文件
                //await SaveUploadedFileAsync(formFile, uploadedFilePath);

                // 解压文件
                await ExtractZipFileAsync(uploadedFilePath, extractPath);

                // 处理并上传文件到MinIO
                var url = await UploadFilesToMinIOAsync(extractPath, uniqueFolderName);
                return url;
            }
            catch (Exception ex)
            {
                Logger.Error($"上传文件处理失败: {ex.Message}", ex);
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, $"上传文件处理失败: {ex.Message}");
            }
            finally
            {
                // 清理临时文件
                await CleanupTempFilesAsync(zipTempPath);
            }
        }

        /// <summary>
        /// 保存上传的文件到临时目录
        /// </summary>
        private async Task SaveUploadedFileAsync(IFormFile file, string filePath)
        {
            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        /// <summary>
        /// 解压ZIP文件到指定目录
        /// </summary>
        private async Task ExtractZipFileAsync(string zipFilePath, string extractPath)
        {
            // 确保目标目录存在且为空
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }
            Directory.CreateDirectory(extractPath);

            // 异步解压文件
            await Task.Run(() =>
            {
                try
                {
                    // 使用 FileStream 打开文件，这样可以更好地控制文件访问
                    using (FileStream zipStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            // 跳过目录条目
                            if (string.IsNullOrEmpty(entry.Name))
                                continue;

                            string destinationPath = Path.Combine(extractPath, entry.FullName);
                            string destinationDirectory = Path.GetDirectoryName(destinationPath);

                            // 创建目标目录（如果不存在）
                            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                            {
                                Directory.CreateDirectory(destinationDirectory);
                            }

                            // 尝试解压文件，带重试机制
                            ExtractFileWithRetry(entry, destinationPath, 3);
                        }
                    }
                }
                catch (InvalidDataException ex)
                {
                    // 处理无效的ZIP文件格式
                    Logger.Error($"无效的ZIP文件格式: {ex.Message}", ex);
                    throw new BizException(BizError.BIND_EXCEPTION_ERROR, "无效的ZIP文件格式，请确保上传的是有效的ZIP压缩文件");
                }
                catch (IOException ex)
                {
                    // 处理IO异常
                    Logger.Error($"读取ZIP文件时发生IO错误: {ex.Message}", ex);
                    throw new BizException(BizError.BIND_EXCEPTION_ERROR, "读取ZIP文件时发生错误，请检查文件是否可访问");
                }
                catch (Exception ex)
                {
                    // 处理其他异常
                    Logger.Error($"解压ZIP文件时发生错误: {ex.Message}", ex);
                    throw new BizException(BizError.BIND_EXCEPTION_ERROR, $"解压ZIP文件时发生错误: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 处理并上传文件到MinIO
        /// </summary>
        private async Task<string> UploadFilesToMinIOAsync(string extractPath, string uniqueFolderName)
        {
            // 处理提取的文件
            var processedFiles = ProcessExtractedFiles(extractPath);

            // 准备上传文件映射
            var uploadFiles = processedFiles.ToDictionary(
                kvp => kvp.Key,
                kvp => uniqueFolderName + "/" + kvp.Value
            );

            string defaultUrl = "";
            var uploadTasks = new List<Task<(string path, string url)>>();

            // 并行上传文件以提高性能
            foreach (var item in uploadFiles)
            {
                var task = Task.Run(async () =>
                {
                    await _minIOFileService.UploadAsync(ContextUser?.TenantAbbreviation.ToLower() ?? "cs0001", item.Value, item.Key);
                    string url = await _minIOFileService.GetFileUrlAsync(ContextUser?.TenantAbbreviation.ToLower() ?? "cs0001", item.Value);

                    return (item.Value, url);
                });
                uploadTasks.Add(task);
            }

            // 等待所有上传任务完成
            var results = await Task.WhenAll(uploadTasks);

            // 查找index.html作为默认URL
            foreach (var result in results)
            {
                if (result.path.Contains("index.html"))
                {
                    defaultUrl = result.url;
                    break;
                }
            }

            // 如果没有找到index.html，使用第一个文件的URL作为默认URL
            if (string.IsNullOrEmpty(defaultUrl) && results.Length > 0)
            {
                defaultUrl = results[0].url;
            }

            return defaultUrl;
        }

        /// <summary>
        /// 清理临时文件
        /// </summary>
        private async Task CleanupTempFilesAsync(string tempPath)
        {
            try
            {
                // 等待一段时间，确保所有文件操作完成
                await Task.Delay(500);
                _tempFolderService.DeleteFiles(tempPath);
            }
            catch (Exception ex)
            {
                // 记录清理错误但不抛出
                Logger.Warn($"清理临时文件时出错: {ex.Message}");
            }
        }

        // 带重试的文件解压方法
        private void ExtractFileWithRetry(ZipArchiveEntry entry, string destinationPath, int maxRetries)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    // 尝试解压文件，覆盖已存在的文件
                    entry.ExtractToFile(destinationPath, true);
                    break; // 成功则退出循环
                }
                catch (IOException ex) when (retryCount < maxRetries &&
                                            (ex.Message.Contains("because it is being used by another process") ||
                                             ex.Message.Contains("already exists")))
                {
                    retryCount++;
                    // 等待一段时间后重试
                    Thread.Sleep(1000);
                }
                catch (Exception)
                {
                    // 其他异常直接抛出
                    throw;
                }
            }
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
