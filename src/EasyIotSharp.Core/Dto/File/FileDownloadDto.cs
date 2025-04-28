using System.IO;

namespace EasyIotSharp.Core.Dto.File
{
    /// <summary>
    /// 下载返回
    /// </summary>
    public class FileDownloadDto
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 文件流
        /// </summary>
        public Stream FileStream { get; set; }

        /// <summary>
        /// 字节
        /// </summary>
        public long FileSize { get; set; }
    }
}