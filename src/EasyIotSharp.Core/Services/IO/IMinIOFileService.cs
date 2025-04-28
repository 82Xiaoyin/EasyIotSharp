using Microsoft.AspNetCore.Http;
using Minio.DataModel.Args;
using Minio.DataModel.Result;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Services.IO
{
    public interface IMinIOFileService
    {
        /// <summary>
        /// 判断 bucket 是否存在
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <returns>是否存在</returns>
        Task<bool> BucketExistsAsync(string bucketName);

        /// <summary>
        /// 创建 bucket
        /// </summary>
        /// <param name="bucketName">桶名</param>
        Task CreateBucketAsync(string bucketName);

        /// <summary>
        /// 获取全部 bucket
        /// </summary>
        /// <returns>Bucket 列表</returns>
        Task<ListAllMyBucketsResult> GetAllBucketsAsync();

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="fileName">文件名</param>
        /// <param name="filePath">文件路径</param>
        Task UploadAsync(string bucketName, string fileName, string filePath);

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="filePath">文件路径</param>
        Task<string> UploadAsync(string fileName, string filePath);

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="fileName">文件名</param>
        /// <param name="stream">文件流</param>
        /// <returns>文件 URL 地址</returns>
        Task<string> UploadAsync(string bucketName, string fileName, Stream stream);

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="stream">文件流</param>
        /// <returns>文件 URL 地址</returns>
        Task<string> UploadAsync(string fileName, Stream stream);

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="file">文件</param>
        /// <returns>文件 URL 地址</returns>
        Task<string> UploadAsync(string bucketName, IFormFile file);

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="file">文件</param>
        /// <returns>文件 URL 地址</returns>
        Task<string> UploadAsync(IFormFile file);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="fileName">文件名</param>
        Task DeleteFileAsync(string bucketName, string fileName);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        Task DeleteFileAsync(string fileName);

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="fileName">文件名</param>
        /// <returns>文件流</returns>
        Task<Stream> DownloadAsync(string bucketName, string fileName);

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件流</returns>
        Task<Stream> DownloadAsync(string fileName);

        /// <summary>
        /// 获取 minio 文件的下载地址
        /// </summary>
        /// <param name="bucketName">桶名</param>
        /// <param name="fileName">文件名</param>
        /// <returns>文件 URL</returns>
        Task<string> GetFileUrlAsync(string bucketName, string fileName);

        /// <summary>
        /// 获取 minio 文件的下载地址
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件 URL</returns>
        Task<string> GetFileUrlAsync(string fileName);

        /// <summary>
        /// 获取文件属性
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>文件属性字典</returns>
        Task<Dictionary<string, object>> GetFileStatAsync(string fileName);

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<Dictionary<string, object>> GetFileStatAsync(string bucketName, string fileName);
    }
}
