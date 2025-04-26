using System;
using System.IO;
using UPrime;

namespace EasyIotSharp.Core.Services.IO.Impl
{
    public class TempFolderService : ITempFolderService
    {
        /// <summary>
        /// 临时文件夹应用绝对路径
        /// </summary>
        public string Path
        {
            get
            {
                var path = Directory.GetCurrentDirectory() + "\\App_Data\\Temp";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        /// <summary>
        /// 临时文件夹导出的Excel应用绝对路径
        /// </summary>
        public string ExportExcelPath
        {
            get
            {
                var path = Path + "\\ExportExcel";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        /// <summary>
        /// 临时文件夹导出的Excel应用绝对路径
        /// </summary>
        public string ZipPath
        {
            get
            {
                var path = Path + "\\Zip";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public string LogPath
        {
            get
            {
                var path = Path + "\\Logs";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        /// <summary>
        /// 通过临时文件夹路径获取绝对路径
        /// </summary>
        /// <param name="ext">后缀名（如：.txt）</param>
        /// <returns></returns>
        public string GetFileWithPath(string ext)
        {
            string path = "{0}\\{1}_{2}{3}".FormatWith(Path, DateTime.Now.ToString("yyMMdd"), Guid.NewGuid().ToString().ReplaceByEmpty("-"), ext);

            return path;
        }

        /// <summary>
        /// 通过临时文件夹路径获取导出Excel的绝对路径
        /// </summary>
        /// <param name="ext">后缀名（如：.xlsx）</param>
        /// <returns></returns>
        public string GetFileWithExportExcelPath(string ext)
        {
            string path = "{0}\\{1}_{2}{3}".FormatWith(ExportExcelPath, DateTime.Now.ToString("yyMMdd"), Guid.NewGuid().ToString().ReplaceByEmpty("-"), ext);

            return path;
        }

        public string GetFileWithExportExcelPath(string ext, string name)
        {
            string path = "{0}\\{1}_{2}{3}".FormatWith(ExportExcelPath, name, Guid.NewGuid().ToString().ReplaceByEmpty("-"), ext);

            return path;
        }

        public string GetFileWithLogPath(string name, string ext)
        {
            string path = "{0}\\{1}.{2}".FormatWith(LogPath, name, ext);

            return path;
        }


        /// <summary>
        /// 获取zip文件夹的绝对路径
        /// </summary>
        /// <param name="extPath"></param>
        /// <returns></returns>
        public string GetZipFolderPath(string extPath)
        {
            var path = "{0}/Zip/{1}".FormatWith(Path, extPath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// 从临时文件夹删除文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ActionOutput DeleteFile(string fileName)
        {
            ActionOutput res = new ActionOutput();
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
                else
                {
                    var path = Path + "\\" + fileName;
                    if (File.Exists(path))
                        File.Delete(path);
                    else
                    {
                        path = ExportExcelPath + "\\" + fileName;
                        if (File.Exists(path))
                            File.Delete(path);
                        else
                        {
                            path = ZipPath+ "\\" + fileName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.AddError(ex.Message);
            }

            return res;
        }

        /// <summary>
        /// 删除文件夹以及下面的所有文件
        /// </summary>
        /// <param name="filePath"></param>
        public void DeleteFiles(string filePath)
        {
            Directory.Delete(filePath, recursive: true);
        }
    }
}