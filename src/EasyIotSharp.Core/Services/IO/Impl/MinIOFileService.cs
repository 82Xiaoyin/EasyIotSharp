using EasyIotSharp.Core.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Minio.DataModel.Args;
using Minio.DataModel;
using Minio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UPrime;
using Minio.DataModel.Result;
using Nest;
using System.Net.Http;
using System.Security.AccessControl;

namespace EasyIotSharp.Core.Services.IO.Impl
{
    public class MinIOFileService : IMinIOFileService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;

        private readonly AppOptions _appOptions;

        public MinIOFileService()
        {
            _appOptions = UPrimeEngine.Instance.Resolve<AppOptions>();
            _bucketName = _appOptions.MinIOOptions.BucketName;
            _minioClient = new MinioClient()
                .WithEndpoint(_appOptions.MinIOOptions.Servers)
                .WithCredentials(_appOptions.MinIOOptions.AccessKey, _appOptions.MinIOOptions.SecretKey)
                .Build();
        }

        /// <summary>
        /// 判断 bucket 是否存在
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <returns>是否存在</returns>
        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            return await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        }

        /// <summary>
        /// 创建 bucket
        /// </summary>
        /// <param name="bucketName">桶名</param>
        public async Task CreateBucketAsync(string bucketName)
        {
            var exists = await BucketExistsAsync(bucketName);
            if (!exists)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
            }
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
        }

        /// <summary>
        /// 获取全部 bucket
        /// </summary>
        /// <returns>Bucket 列表</returns>
        public async Task<ListAllMyBucketsResult> GetAllBucketsAsync()
        {
            return await _minioClient.ListBucketsAsync();
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="fileName">文件名</param>
        /// <param name="filePath">文件路径</param>
        public async Task UploadAsync(string bucketName, string fileName, string filePath)
        {
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithFileName(filePath);
            await _minioClient.PutObjectAsync(putObjectArgs);
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="filePath">文件路径</param>
        public async Task UploadAsync(string fileName, string filePath)
        {
            await UploadAsync(_bucketName, fileName, filePath);
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
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("application/octet-stream");

            await _minioClient.PutObjectAsync(putObjectArgs);
            return await GetFileUrlAsync(bucketName, fileName);
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="stream">文件流</param>
        /// <returns>文件 URL 地址</returns>
        public async Task<string> UploadAsync(string fileName, Stream stream)
        {
            return await UploadAsync(_bucketName, fileName, stream);
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="file">文件</param>
        /// <returns>文件 URL 地址</returns>
        public async Task<string> UploadAsync(string bucketName, IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var fileName = file.FileName;
            return await UploadAsync(bucketName, fileName, stream);
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="file">文件</param>
        /// <returns>文件 URL 地址</returns>
        public async Task<string> UploadAsync(IFormFile file)
        {
            await CreateBucketAsync(_bucketName);

            return await UploadAsync(_bucketName, file);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="fileName">文件名</param>
        public async Task DeleteFileAsync(string bucketName, string fileName)
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName));
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
        public async Task<Stream> DownloadAsync(string bucketName, string fileName)
        {
            var stream = new MemoryStream();
            var args = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithCallbackStream(s => s.CopyTo(stream));

            await _minioClient.GetObjectAsync(args);
            stream.Position = 0;
            return stream;
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
            var policy = await _minioClient.GetPolicyAsync(
                     new GetPolicyArgs().WithBucket(_bucketName));
            if (policy.IsNotNull())
            {
                bool useHttps = false;
                return $"{(useHttps ? "https" : "http")}://{_appOptions.MinIOOptions.Servers}/{bucketName}/{fileName}";
            }
            else
            {
                var args = new PresignedGetObjectArgs()
                 .WithBucket(bucketName)
                 .WithObject(fileName)
                 .WithExpiry(60 * 60 * 24); // 24小时有效期

                return await _minioClient.PresignedGetObjectAsync(args);
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
            var stat = await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName));

            return new Dictionary<string, object>
            {
                ["length"] = stat.Size,
                ["createTime"] = stat.LastModified,
                ["bucketName"] = bucketName,
                ["fileName"] = stat.ObjectName
            };
        }
    }
}
