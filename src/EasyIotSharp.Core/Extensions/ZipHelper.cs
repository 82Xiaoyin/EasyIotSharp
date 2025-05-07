using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace EasyIotSharp.Core.Extensions
{
    /// <summary>
    /// 导出zip文件拓展类
    /// </summary>
    public class ZipHelper
    {
        /// <summary>
        /// 单文件压缩
        /// </summary>
        /// <param name="sourceFile">源文件</param>
        /// <param name="zipedFile">zip压缩文件</param>
        /// <param name="blockSize">缓冲区大小</param>
        /// <param name="compressionLevel">压缩级别</param>
        public static void ZipFile(string sourceFile, string zipedFile, int blockSize = 1024, int compressionLevel = 6)
        {
            if (!File.Exists(sourceFile))
            {
                throw new System.IO.FileNotFoundException("The specified file " + sourceFile + " could not be found.");
            }
            var fileName = System.IO.Path.GetFileNameWithoutExtension(sourceFile) + ".csv";

            FileStream streamToZip = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            FileStream zipFile = File.Create(zipedFile);
            ZipOutputStream zipStream = new ZipOutputStream(zipFile);

            ZipEntry zipEntry = new ZipEntry(fileName);
            zipStream.PutNextEntry(zipEntry);

            //存储、最快、较快、标准、较好、最好  0-9
            zipStream.SetLevel(compressionLevel);

            byte[] buffer = new byte[blockSize];

            int size = streamToZip.Read(buffer, 0, buffer.Length);
            zipStream.Write(buffer, 0, size);
            try
            {
                while (size < streamToZip.Length)
                {
                    int sizeRead = streamToZip.Read(buffer, 0, buffer.Length);
                    zipStream.Write(buffer, 0, sizeRead);
                    size += sizeRead;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            zipStream.Finish();
            zipStream.Close();
            streamToZip.Close();
        }
        /// <summary>
        /// 多文件压缩
        /// </summary>
        /// <param name="zipfile">zip压缩文件</param>
        /// <param name="filenames">源文件集合</param>
        /// <param name="password">压缩加密</param>
        public static void ZipFiles(string zipfile, string[] filenames, string password = "")
        {
            ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(zipfile));

            s.SetLevel(6);

            foreach (string file in filenames)
            {
                //打开压缩文件
                FileStream fs = File.OpenRead(file);

                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);

                var name = Path.GetFileName(file);

                ZipEntry entry = new ZipEntry(name);
                entry.DateTime = DateTime.Now;
                entry.Size = fs.Length;
                fs.Close();
                s.PutNextEntry(entry);
                s.Write(buffer, 0, buffer.Length);
            }
            s.Finish();
            s.Close();
        }

    }
}
