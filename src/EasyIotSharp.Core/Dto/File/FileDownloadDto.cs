using System.IO;

namespace EasyIotSharp.Core.Dto.File
{
    /// <summary>
    /// 文件下载数据传输对象
    /// </summary>
    public class FileDownloadDto
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 文件流
        /// </summary>
        public Stream FileStream { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }
    }
}