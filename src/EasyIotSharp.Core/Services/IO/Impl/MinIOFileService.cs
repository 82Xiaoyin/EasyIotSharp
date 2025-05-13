using EasyIotSharp.Core.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio.DataModel.Args;
using Minio.DataModel;
using Minio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UPrime;
using Minio.DataModel.Result;

namespace EasyIotSharp.Core.Services.IO.Impl
{
    public class MinIOFileService : IMinIOFileService, IDisposable
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        private readonly AppOptions _appOptions;
        private readonly ILogger<MinIOFileService> _logger;
        
        // 缓存URL，提高性能
        private readonly ConcurrentDictionary<string, Tuple<string, DateTime>> _urlCache = 
            new ConcurrentDictionary<string, Tuple<string, DateTime>>();

        public MinIOFileService(ILogger<MinIOFileService> logger = null)
        {
            _appOptions = UPrimeEngine.Instance.Resolve<AppOptions>();
            _bucketName = _appOptions.MinIOOptions.BucketName;
            _logger = logger;
            
            try
            {
                _minioClient = new MinioClient()
                    .WithEndpoint(_appOptions.MinIOOptions.Servers)
                    .WithCredentials(_appOptions.MinIOOptions.AccessKey, _appOptions.MinIOOptions.SecretKey)
                    .Build();
                
                _logger?.LogInformation("MinIO客户端初始化成功");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "MinIO客户端初始化失败");
                throw;
            }
        }

        #region 桶操作

        /// <summary>
        /// 判断 bucket 是否存在
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <returns>是否存在</returns>
        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            try
            {
                return await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"检查桶 {bucketName} 是否存在时出错");
                throw;
            }
        }

        /// <summary>
        /// 创建 bucket
        /// </summary>
        /// <param name="bucketName">桶名</param>
        public async Task CreateBucketAsync(string bucketName)
        {
            try
            {
                var exists = await BucketExistsAsync(bucketName);
                if (!exists)
                {
                    _logger?.LogInformation($"创建桶: {bucketName}");
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
                }
                
                // 设置桶策略，允许公共访问
                var policy = @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {
                            ""Effect"": ""Allow"",
                            ""Principal"": {""AWS"": [""*""]},
                            ""Action"": [
                                ""s3:GetBucketLocation"",
                                ""s3:ListBucket"",
                                ""s3:GetObject""
                            ],
                            ""Resource"": [
                                ""arn:aws:s3:::" + bucketName + @""",
                                ""arn:aws:s3:::" + bucketName + @"/*""
                            ]
                        }
                    ]
                }";

                await _minioClient.SetPolicyAsync(
                    new SetPolicyArgs()
                        .WithBucket(bucketName)
                        .WithPolicy(policy));
                
                _logger?.LogInformation($"桶 {bucketName} 策略设置成功");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"创建桶 {bucketName} 时出错");
                throw;
            }
        }

        /// <summary>
        /// 获取全部 bucket
        /// </summary>
        /// <returns>Bucket 列表</returns>
        public async Task<ListAllMyBucketsResult> GetAllBucketsAsync()
        {
            try
            {
                return await _minioClient.ListBucketsAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "获取所有桶列表时出错");
                throw;
            }
        }

        #endregion

        #region 文件上传

        /// <summary>
        /// 核心上传方法
        /// </summary>
        private async Task<string> CoreUploadAsync(string bucketName, string fileName, Stream stream, string contentType = null)
        {
            try
            {
                // 确保桶存在
                await CreateBucketAsync(bucketName);
                
                // 如果未指定内容类型，则根据文件扩展名确定
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = GetContentType(Path.GetExtension(fileName));
                }
                
                // 验证文件类型
                if (!ValidateFileType(fileName))
                {
                    throw new InvalidOperationException($"不支持的文件类型: {Path.GetExtension(fileName)}");
                }
                
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(putObjectArgs);
                _logger?.LogInformation($"文件 {fileName} 上传到 {bucketName} 成功");
                
                return await GetFileUrlAsync(bucketName, fileName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"上传文件 {fileName} 到 {bucketName} 失败");
                throw;
            }
        }

        /// <summary> 
        /// 文件上传 
        /// </summary> 
        /// <param name="bucketName">桶名</param> 
        /// <param name="fileName">文件名</param> 
        /// <param name="filePath">文件路径</param> 
        public async Task UploadAsync(string bucketName, string fileName, string filePath)
        {
            try
            {
                // 确保桶存在
                await CreateBucketAsync(bucketName);
                
                // 根据文件扩展名确定 Content-Type
                string contentType = GetContentType(Path.GetExtension(filePath));

                // 验证文件类型
                if (!ValidateFileType(fileName))
                {
                    throw new InvalidOperationException($"不支持的文件类型: {Path.GetExtension(fileName)}");
                }

                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithFileName(filePath)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(putObjectArgs);
                _logger?.LogInformation($"文件 {fileName} 从路径 {filePath} 上传到 {bucketName} 成功");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"上传文件 {fileName} 从路径 {filePath} 到 {bucketName} 失败");
                throw;
            }
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="filePath">文件路径</param>
        public async Task<string> UploadAsync(string fileName, string filePath)
        {
            try
            {
                await UploadAsync(_bucketName, fileName, filePath);
                return await GetFileUrlAsync(_bucketName, fileName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"上传文件 {fileName} 从路径 {filePath} 失败");
                throw;
            }
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="fileName">文件名</param>
        /// <param name="stream">文件流</param>
        /// <returns>文件 URL 地址</returns>
        public async Task<string> UploadAsync(string bucketName, string fileName, Stream stream)
        {
            return await CoreUploadAsync(bucketName, fileName, stream);
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="stream">文件流</param>
        /// <returns>文件 URL 地址</returns>
        public async Task<string> UploadAsync(string fileName, Stream stream)
        {
            return await CoreUploadAsync(_bucketName, fileName, stream);
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="file">文件</param>
        /// <returns>文件 URL 地址</returns>
        public async Task<string> UploadAsync(string bucketName, IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var fileName = file.FileName;
                return await CoreUploadAsync(bucketName, fileName, stream, file.ContentType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"上传表单文件 {file.FileName} 到 {bucketName} 失败");
                throw;
            }
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="file">文件</param>
        /// <returns>文件 URL 地址</returns>
        public async Task<string> UploadAsync(IFormFile file)
        {
            try
            {
                _logger?.LogInformation($"开始上传文件: {file.FileName}, 大小: {file.Length} 字节");
                await CreateBucketAsync(_bucketName);
                var result = await UploadAsync(_bucketName, file);
                _logger?.LogInformation($"文件上传成功: {file.FileName}, URL: {result}");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"上传表单文件 {file.FileName} 失败");
                throw;
            }
        }

        /// <summary>
        /// 大文件分片上传
        /// </summary>
        public async Task<string> UploadLargeFileAsync(string fileName, Stream stream, int partSize = 5 * 1024 * 1024)
        {
            return await UploadLargeFileAsync(_bucketName, fileName, stream, partSize);
        }

        /// <summary>
        /// 大文件分片上传
        /// </summary>
        public async Task<string> UploadLargeFileAsync(string bucketName, string fileName, Stream stream, int partSize = 5 * 1024 * 1024)
        {
            try
            {
                await CreateBucketAsync(bucketName);
                
                string contentType = GetContentType(Path.GetExtension(fileName));
                
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType)
                    .WithObjectSize(partSize);

                await _minioClient.PutObjectAsync(putObjectArgs);
                _logger?.LogInformation($"大文件 {fileName} 分片上传到 {bucketName} 成功, 大小: {stream.Length} 字节");
                
                return await GetFileUrlAsync(bucketName, fileName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"大文件 {fileName} 分片上传失败");
                throw;
            }
        }

        #endregion

        #region 文件操作

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="fileName">文件名</param>
        public async Task DeleteFileAsync(string bucketName, string fileName)
        {
            try
            {
                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName));
                
                // 从缓存中移除
                string cacheKey = $"{bucketName}:{fileName}";
                _urlCache.TryRemove(cacheKey, out _);
                
                _logger?.LogInformation($"文件 {fileName} 从 {bucketName} 删除成功");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"删除文件 {fileName} 从 {bucketName} 失败");
                throw;
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        public async Task DeleteFileAsync(string fileName)
        {
            await DeleteFileAsync(_bucketName, fileName);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="fileName">文件名</param>
        /// <returns>文件流</returns>
        /// <summary> 
        public async Task<Stream> DownloadAsync(string bucketName, string fileName)
        {
            try
            {
                _logger?.LogInformation($"开始下载文件 {fileName} 从 {bucketName}");

                var stream = new MemoryStream();
                var args = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithCallbackStream(s => s.CopyTo(stream));

                await _minioClient.GetObjectAsync(args);
                stream.Position = 0;
                _logger?.LogInformation($"文件 {fileName} 从 {bucketName} 下载成功");
                return stream;
            }
            catch (Minio.Exceptions.ObjectNotFoundException ex)
            {
                _logger?.LogWarning($"文件 {fileName} 在存储桶 {bucketName} 中不存在");

                // 尝试URL编码文件名再次尝试
                try
                {
                    string encodedFileName = string.Join("/",
                        fileName.Split('/').Select(segment => Uri.EscapeDataString(segment)));

                    _logger?.LogInformation($"尝试使用编码后的文件名下载: {encodedFileName}");

                    var stream = new MemoryStream();
                    var args = new GetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(encodedFileName)
                        .WithCallbackStream(s => s.CopyTo(stream));

                    await _minioClient.GetObjectAsync(args);
                    stream.Position = 0;
                    _logger?.LogInformation($"使用编码后的文件名下载成功");
                    return stream;
                }
                catch (Exception innerEx)
                {
                    _logger?.LogError(innerEx, $"使用编码后的文件名下载失败");
                    throw ex; // 抛出原始异常
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"下载文件 {fileName} 从 {bucketName} 失败");
                throw;
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件流</returns>
        public async Task<Stream> DownloadAsync(string fileName)
        {
            return await DownloadAsync(_bucketName, fileName);
        }

        /// <summary>
        /// 获取 minio 文件的下载地址
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="fileName">文件名</param>
        /// <returns>文件 URL</returns>
        public async Task<string> GetFileUrlAsync(string bucketName, string fileName)
        {
            try
            {
                string cacheKey = $"{bucketName}:{fileName}";
                
                // 检查缓存
                if (_urlCache.TryGetValue(cacheKey, out var cachedValue))
                {
                    // 如果缓存未过期，直接返回
                    if (cachedValue.Item2 > DateTime.Now)
                    {
                        return cachedValue.Item1;
                    }
                    // 缓存已过期，移除
                    _urlCache.TryRemove(cacheKey, out _);
                }
                
                var policy = await _minioClient.GetPolicyAsync(
                         new GetPolicyArgs().WithBucket(bucketName));
                
                string url;
                if (policy != null)
                {
                    bool useHttps = _appOptions.MinIOOptions.UseHttps;
                    url = $"{(useHttps ? "https" : "http")}://{_appOptions.MinIOOptions.Servers}/{bucketName}/{fileName}";
                }
                else
                {
                    var args = new PresignedGetObjectArgs()
                     .WithBucket(bucketName)
                     .WithObject(fileName)
                     .WithExpiry(_appOptions.MinIOOptions.UrlExpiryHours * 60 * 60); // 可配置的过期时间

                    url = await _minioClient.PresignedGetObjectAsync(args);
                    
                    // 缓存URL，有效期比预签名URL短一些
                    _urlCache[cacheKey] = new Tuple<string, DateTime>(
                        url, 
                        DateTime.Now.AddHours(_appOptions.MinIOOptions.UrlExpiryHours - 1)
                    );
                }
                
                return url;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"获取文件 {fileName} 从 {bucketName} 的URL失败");
                throw;
            }
        }

        /// <summary>
        /// 获取 minio 文件的下载地址
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件 URL</returns>
        public async Task<string> GetFileUrlAsync(string fileName)
        {
            return await GetFileUrlAsync(_bucketName, fileName);
        }

        /// <summary>
        /// 获取文件属性
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>文件属性字典</returns>
        public async Task<Dictionary<string, object>> GetFileStatAsync(string fileName)
        {
            return await GetFileStatAsync(_bucketName, fileName);
        }

        /// <summary>
        /// 获取文件属性
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="fileName">文件名称</param>
        /// <returns>文件属性字典</returns>
        public async Task<Dictionary<string, object>> GetFileStatAsync(string bucketName, string fileName)
        {
            try
            {
                var stat = await _minioClient.StatObjectAsync(new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName));

                return new Dictionary<string, object>
                {
                    ["length"] = stat.Size,
                    ["createTime"] = stat.LastModified,
                    ["bucketName"] = bucketName,
                    ["fileName"] = stat.ObjectName,
                    ["contentType"] = stat.ContentType,
                    ["etag"] = stat.ETag
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"获取文件 {fileName} 从 {bucketName} 的属性失败");
                throw;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 根据文件扩展名获取对应的 Content-Type
        /// </summary>
        /// <param name="extension">文件扩展名</param>
        /// <returns>对应的 Content-Type</returns>
        private string GetContentType(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return "application/octet-stream";
                
            switch (extension.ToLower())
            {
                case ".html":
                case ".htm":
                    return "text/html";
                case ".js":
                    return "application/javascript";
                case ".css":
                    return "text/css";
                case ".json":
                    return "application/json";
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".svg":
                    return "image/svg+xml";
                case ".wasm":
                    return "application/wasm";
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls":
                    return "application/vnd.ms-excel";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".ppt":
                    return "application/vnd.ms-powerpoint";
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".zip":
                    return "application/zip";
                case ".rar":
                    return "application/x-rar-compressed";
                case ".7z":
                    return "application/x-7z-compressed";
                case ".mp3":
                    return "audio/mpeg";
                case ".mp4":
                    return "video/mp4";
                case ".avi":
                    return "video/x-msvideo";
                case ".txt":
                    return "text/plain";
                case ".csv":
                    return "text/csv";
                case ".xml":
                    return "application/xml";
                case ".unityweb":
                    return "application/octet-stream"; // Unity WebGL 特定文件
                case ".data":
                    return "application/octet-stream";
                case ".mem":
                    return "application/octet-stream";
                default:
                    return "application/octet-stream";
            }
        }

        /// <summary>
        /// 验证文件类型是否允许
        /// </summary>
        private bool ValidateFileType(string fileName)
        {
            // 如果未启用文件类型验证，则直接返回true
            if (!_appOptions.MinIOOptions.EnableFileTypeValidation)
                return true;
                
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            // 如果配置了允许的扩展名列表，则使用配置的列表
            if (_appOptions.MinIOOptions.AllowedExtensions != null && _appOptions.MinIOOptions.AllowedExtensions.Any())
            {
                return _appOptions.MinIOOptions.AllowedExtensions.Contains(extension);
            }
            
            // 默认允许的扩展名列表
            var allowedExtensions = new[] { 
                ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", 
                ".xls", ".xlsx", ".ppt", ".pptx", ".zip", ".rar", ".7z", 
                ".txt", ".csv", ".xml", ".html", ".htm", ".js", ".css", 
                ".json", ".svg", ".mp3", ".mp4", ".avi" 
            };
            
            return allowedExtensions.Contains(extension);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            // 清理缓存
            _urlCache?.Clear();
            _logger?.LogInformation("MinIO文件服务已释放资源");
        }

        #endregion
    }
}
