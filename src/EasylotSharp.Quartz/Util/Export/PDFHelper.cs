using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace EasylotSharp.Quartz.Util.Export
{
    /// <summary>
    /// PDF生成帮助类
    /// 用于生成简单的PDF文档，支持中文字体和文本换行
    /// </summary>
    public class PDFHelper
    {
        /// <summary>
        /// 处理文本换行的辅助方法
        /// </summary>
        /// <param name="gfx">PDF图形上下文</param>
        /// <param name="text">需要绘制的文本内容</param>
        /// <param name="font">字体设置</param>
        /// <param name="brush">画笔颜色</param>
        /// <param name="position">起始位置坐标</param>
        private void DrawMultilineText(XGraphics gfx, string text, XFont font, XBrush brush, XPoint position)
        {
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            double lineHeight = font.Height * 1.5; // 1.5倍行距
            double currentY = position.Y;

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    // 空行也要占据一行的高度
                    currentY += lineHeight;
                    continue;
                }

                gfx.DrawString(line, font, brush, new XPoint(position.X, currentY));
                currentY += lineHeight;
            }
        }

        /// <summary>
        /// 生成简单的PDF文档
        /// </summary>
        /// <param name="text">要写入PDF的文本内容</param>
        /// <param name="fileName">PDF文件名（包含扩展名）</param>
        /// <returns>包含PDF内容的内存流</returns>
        /// <exception cref="Exception">生成PDF过程中的异常</exception>
        /// <remarks>
        /// 该方法会：
        /// 1. 创建一个新的PDF文档
        /// 2. 添加标题、正文和页脚
        /// 3. 支持中文字体显示
        /// 4. 同时保存到文件系统和返回内存流
        /// </remarks>
        public MemoryStream GenerateSimplePdf(string text, string fileName)
        {
            try
            {
                // 设置中文编码支持
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // 设置字体解析器，用于支持中文字体
                if (GlobalFontSettings.FontResolver == null)
                {
                    GlobalFontSettings.FontResolver = new CustomFontResolver();
                }

                // 创建内存流，用于存储PDF内容
                var memoryStream = new MemoryStream();

                // 设置文件保存路径
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string filesDirectory = Path.Combine(currentDirectory, "Files");
                Directory.CreateDirectory(filesDirectory);
                string outputPath = Path.Combine(filesDirectory, fileName);

                // 创建PDF文档并设置内容
                using (var document = new PdfDocument())
                {
                    var page = document.AddPage();
                    using (var gfx = XGraphics.FromPdfPage(page))
                    {
                        // 设置标题和正文字体
                        var titleFont = new XFont("FangSong", 20);
                        var contentFont = new XFont("FangSong", 12);

                        // 绘制文档标题
                        gfx.DrawString("文档内容", titleFont, XBrushes.Black,
                            new XPoint(50, 50));

                        // 绘制正文内容（支持换行）
                        DrawMultilineText(gfx, text, contentFont, XBrushes.Black,
                            new XPoint(50, 100));

                        // 添加页脚（生成时间）
                        gfx.DrawString($"生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                            contentFont, XBrushes.Gray,
                            new XPoint(50, page.Height - 50));
                    }
                    // 保存文档到内存流
                    document.Save(memoryStream);
                    memoryStream.Position = 0; // 重置流位置到开始
                }

                // 如果提供了文件名，则同时保存到文件系统
                if (!string.IsNullOrEmpty(fileName))
                {
                    using (var fileStream = File.Create(outputPath))
                    {
                        memoryStream.Position = 0;
                        memoryStream.CopyTo(fileStream);
                    }
                    Console.WriteLine($"PDF文件已保存到: {outputPath}");
                }

                Console.WriteLine($"PDF文件已保存到内存流中");
                return memoryStream;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"生成PDF时发生错误: {ex.Message}");
                throw;
            }
        }
    }
}
