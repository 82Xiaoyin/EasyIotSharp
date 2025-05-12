using PdfSharp.Fonts;
using System;
using System.IO;

namespace EasylotSharp.Quartz.Util.Export
{
    /// <summary>
    /// 自定义字体解析器
    /// 用于解析和加载PDF文档中使用的中文字体
    /// 支持仿宋体、宋体、黑体等常用中文字体
    /// </summary>
    public class CustomFontResolver : IFontResolver
    {
        /// <summary>
        /// 获取字体数据
        /// </summary>
        /// <param name="faceName">字体名称</param>
        /// <returns>字体文件的字节数组</returns>
        /// <remarks>
        /// 支持的字体类型：
        /// - FangSong (仿宋体)
        /// - SimSun (宋体)
        /// - SimHei (黑体)
        /// 如果未指定或不支持的字体，将默认使用宋体
        /// </remarks>
        public byte[] GetFont(string faceName)
        {
            // 根据字体名称返回对应的字体文件
            switch (faceName.ToLower())
            {
                case "fangsong":
                    return LoadFontData("SIMFANG.TTF");
                case "simsun":
                    return LoadFontData("SIMSUN.TTC");
                case "simhei":
                    return LoadFontData("SIMHEI.TTF");
                default:
                    return LoadFontData("SIMSUN.TTC"); // 默认使用宋体
            }
        }

        /// <summary>
        /// 解析字体类型信息
        /// </summary>
        /// <param name="familyName">字体族名称</param>
        /// <param name="isBold">是否粗体</param>
        /// <param name="isItalic">是否斜体</param>
        /// <returns>字体解析信息</returns>
        /// <remarks>
        /// 返回字体的基本信息，用于PDFSharp的字体渲染系统
        /// </remarks>
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // 返回字体信息
            return new FontResolverInfo(familyName);
        }

        /// <summary>
        /// 加载字体文件数据
        /// </summary>
        /// <param name="fontFileName">字体文件名</param>
        /// <returns>字体文件的字节数组</returns>
        /// <remarks>
        /// 1. 首先尝试从Windows系统字体目录加载指定的字体文件
        /// 2. 如果指定的字体文件不存在，则使用系统默认的宋体(SIMSUN.TTC)作为备选
        /// 3. 字体文件路径通常为：C:\Windows\Fonts\
        /// </remarks>
        private byte[] LoadFontData(string fontFileName)
        {
            // 从Windows字体目录加载字体文件
            string winFontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts", fontFileName);
            
            if (File.Exists(winFontPath))
            {
                return File.ReadAllBytes(winFontPath);
            }
            
            // 如果找不到字体文件，使用备用字体
            string defaultFontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts", "SIMSUN.TTC");
            return File.ReadAllBytes(defaultFontPath);
        }
    }
}