using UPrime;
using UPrime.Dependency;

namespace EasyIotSharp.Core.Services.IO
{
    public interface ITempFolderService : ISingletonDependency
    {
        /// <summary>
        /// 临时文件夹应用绝对路径
        /// </summary>
        string Path { get; }

        /// <summary>
        /// 临时文件夹导出的Excel应用绝对路径
        /// </summary>
        string ExportExcelPath { get; }

        /// <summary>
        /// 通过临时文件夹路径获取绝对路径
        /// </summary>
        /// <param name="ext">后缀名（如：.txt）</param>
        /// <returns></returns>
        string GetFileWithPath(string ext);

        /// <summary>
        /// 通过临时文件夹路径获取导出Excel的绝对路径
        /// </summary>
        /// <param name="ext">后缀名（如：.xlsx）</param>
        /// <returns></returns>
        string GetFileWithExportExcelPath(string ext);

        /// <summary>
        /// 通过临时文件夹路径获取导出Excel的绝对路径
        /// </summary>
        /// <param name="ext">后缀名（如：.xlsx）</param>
        /// <param name="name">文件名称</param>
        /// <returns></returns>
        string GetFileWithExportExcelPath(string ext, string name);

        /// <summary>
        /// 通过临时文件夹路径获取慢查询日志存储的绝对路径
        /// </summary>
        /// <param name="name">日志名称</param>
        /// <param name="ext">后缀名（如：.log）</param>
        /// <returns></returns>
        string GetFileWithLogPath(string name, string ext);

        /// <summary>
        /// 获取zip文件夹的绝对路径
        /// </summary>
        /// <param name="extPath"></param>
        /// <returns></returns>
        string GetZipFolderPath(string extPath);

        /// <summary>
        /// 从临时文件夹删除文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        ActionOutput DeleteFile(string fileName);

        /// <summary>
        /// 删除文件夹以及下面的所有的文件
        /// </summary>
        /// <param name="filePath"></param>
        void DeleteFiles(string filePath);
    }
}