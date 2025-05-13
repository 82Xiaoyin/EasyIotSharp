using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.IO.Font.Constants;
using System.IO;
using System;
using ScottPlot;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iText.Kernel.Colors;

namespace EasylotSharp.Quartz.Util.Export
{
    public  class PDFHelper
    {
        // 添加字体缓存
        private  PdfFont _simsunFont;

        // 获取中文字体
        private  PdfFont GetChineseFont()
        {
            if (_simsunFont == null)
            {
                try
                {
                    // 尝试加载系统字体
                    var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "SIMHEI.TTF");
                    if (File.Exists(fontPath))
                    {
                        _simsunFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
                    }
                    else
                    {
                        // 如果黑体不存在，尝试宋体
                        fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "SIMSUN.TTC,0");
                        if (File.Exists(fontPath))
                        {
                            _simsunFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
                        }
                        else
                        {
                            // 如果系统字体都不存在，使用默认字体
                            _simsunFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.IDENTITY_H);
                            Console.WriteLine("警告：未能加载中文字体，将使用默认字体。某些中文字符可能无法正确显示。");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载字体时出错: {ex.Message}");
                    // 使用最基本的字体作为后备方案
                    _simsunFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.IDENTITY_H);
                }
            }
            return _simsunFont;
        }

        public  byte[] GenerateHourlyAlarmDayChart(
            IEnumerable<(int Hour, int Count)> hourlyData,
            string title,
            int width = 800,
            int height = 400)
        {
            // 检查数据是否为空
            if (hourlyData == null || !hourlyData.Any())
            {
                return null;
            }

            // 创建新的图表
            var plt = new Plot(width, height);

            // 确保数据按小时排序
            var sortedData = hourlyData.OrderBy(x => x.Hour).ToList();

            // 准备数据
            double[] hours = sortedData.Select(x => (double)x.Hour).ToArray();
            double[] counts = sortedData.Select(x => (double)x.Count).ToArray();

            // 使用更简单的 API 绘制折线图
            plt.PlotScatter(hours, counts,
                label: "告警数量",
                lineWidth: 2,
                markerSize: 5,
                color: System.Drawing.Color.Blue);

            // 设置标题和标签
            plt.Title(title);
            plt.XLabel("时间（小时）");
            plt.YLabel("告警数量");

            // 设置 X 轴范围和刻度
            plt.SetAxisLimits(xMin: -0.5, xMax: 23.5);
            plt.XAxis.ManualTickPositions(
                Enumerable.Range(0, 24).Select(i => (double)i).ToArray(),
                Enumerable.Range(0, 24).Select(i => $"{i}时").ToArray());

            // 添加网格线
            plt.Grid(true);

            // 添加图例
            plt.Legend();

            // 导出为图片
            string chartDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExportFiles");
            // 确保目录存在
            if (!Directory.Exists(chartDirectory))
            {
                Directory.CreateDirectory(chartDirectory);
            }
            string tempFilePath = Path.Combine(chartDirectory, $"chart_{Guid.NewGuid()}.png");
            try
            {
                // 先保存到文件
                plt.SaveFig(tempFilePath);

                // 读取文件内容
                byte[] imageBytes = File.ReadAllBytes(tempFilePath);

                return imageBytes;
            }
            finally
            {
                // 确保临时文件被删除
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
        /// <summary>
        /// 处理文本换行的辅助方法
        /// </summary>
        private  string ProcessMultilineText(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            // 替换Windows和Unix风格的换行符为统一格式
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            // 处理连续的换行符
            var lines = text.Split('\n');
            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    // 空行添加一个换行符
                    sb.AppendLine();
                }
                else
                {
                    // 非空行添加内容和换行符
                    sb.AppendLine(line.Trim());
                }
            }

            return sb.ToString().TrimEnd();
        }

        public  byte[] GenerateSimplePdf(string content, string title, byte[] chartImage = null)
        {
            if (string.IsNullOrEmpty(content)) throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));

            // 处理文本内容，确保换行符的正确处理
            content = ProcessMultilineText(content);

            byte[] result;
            using (var memoryStream = new MemoryStream())
            {
                // 创建PDF文档
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                try
                {
                    // 获取中文字体
                    var chineseFont = GetChineseFont();

                    // 添加标题
                    document.Add(new Paragraph(title)
                        .SetFont(chineseFont)
                        .SetFontSize(16)
                        .SetBold()
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    // 添加内容（确保内容被正确添加）
                    foreach (var line in content.Split('\n'))
                    {
                        var contentParagraph = new Paragraph(line)
                            .SetFont(chineseFont)
                            .SetFontSize(12)
                            .SetMarginTop(5)
                            .SetMarginBottom(5);
                        document.Add(contentParagraph);
                    }

                    // 如果有图表数据，添加图表
                    if (chartImage != null && chartImage.Length > 0)
                    {
                        // 添加图表标题
                        document.Add(new Paragraph("告警趋势分析")
                            .SetFont(chineseFont)
                            .SetFontSize(14)
                            .SetBold()
                            .SetMarginTop(20)
                            .SetMarginBottom(10)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                        // 添加图表
                        var imageData = ImageDataFactory.Create(chartImage);
                        document.Add(new Image(imageData)
                            .SetWidth(500)
                            .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER));
                    }
                    else
                    {
                        document.Add(new Paragraph("无告警数据")
                           .SetFont(chineseFont)
                           .SetFontSize(12)
                           .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                           .SetFontColor(ColorConstants.GRAY));
                    }

                    // 添加页脚
                    document.Add(new Paragraph($"生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                        .SetFont(chineseFont)
                        .SetFontSize(10)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                        .SetFontColor(ColorConstants.GRAY));

                    // 关闭文档
                    document.Close();
                    pdf.Close();
                    writer.Close();

                    // 获取生成的PDF内容
                    result = memoryStream.ToArray();
                }
                catch (Exception ex)
                {
                    throw new Exception($"生成PDF文档失败: {ex.Message}", ex);
                }
            }

            return result;
        }
    }
}
