using System.IO;

namespace EasyIotSharp.Core.Dto.File
{
    /// <summary>
    /// ���ط���
    /// </summary>
    public class FileDownloadDto
    {
        /// <summary>
        /// ����
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// �ļ���
        /// </summary>
        public Stream FileStream { get; set; }

        /// <summary>
        /// �ֽ�
        /// </summary>
        public long FileSize { get; set; }
    }
}