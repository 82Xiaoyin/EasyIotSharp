//using ScottPlot;
//using System.Collections.Generic;
//using System.Linq;
//using System.IO;
//using System;
//using iText.IO.Image;
//using iText.Kernel.Pdf;
//using iText.Layout.Element;
//using iText.Layout;

//namespace EasylotSharp.Quartz.Util.Export
//{
//    public static class ChartHelper
//    {
//        public static byte[] GenerateHourlyAlarmChart(
//            IEnumerable<(int Hour, int Count)> hourlyData,
//            string title,
//            int width = 800,
//            int height = 400)
//        {
//            // 创建新的图表
//            var plt = new Plot(width, height);

//            // 确保数据按小时排序
//            var sortedData = hourlyData.OrderBy(x => x.Hour).ToList();
            
//            // 准备数据
//            double[] hours = sortedData.Select(x => (double)x.Hour).ToArray();
//            double[] counts = sortedData.Select(x => (double)x.Count).ToArray();

//            // 使用更简单的 API 绘制折线图
//            plt.PlotScatter(hours, counts, 
//                label: "告警数量", 
//                lineWidth: 2, 
//                markerSize: 5, 
//                color: System.Drawing.Color.Blue);

//            // 设置标题和标签
//            plt.Title(title);
//            plt.XLabel("时间（小时）");
//            plt.YLabel("告警数量");

//            // 设置 X 轴范围和刻度
//            plt.SetAxisLimits(xMin: -0.5, xMax: 23.5);
//            plt.XAxis.ManualTickPositions(
//                Enumerable.Range(0, 24).Select(i => (double)i).ToArray(),
//                Enumerable.Range(0, 24).Select(i => $"{i}时").ToArray());

//            // 添加网格线
//            plt.Grid(true);

//            // 添加图例
//            plt.Legend();

//            // 导出为图片
//            string chartDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExportFiles");
//            // 确保目录存在
//            if (!Directory.Exists(chartDirectory))
//            {
//                Directory.CreateDirectory(chartDirectory);
//            }
//            string tempFilePath = Path.Combine(chartDirectory, $"chart_{Guid.NewGuid()}.png");
//            try
//            {
//                // 先保存到文件
//                plt.SaveFig(tempFilePath);

//                // 读取文件内容
//                byte[] imageBytes = File.ReadAllBytes(tempFilePath);

//                return imageBytes;
//            }
//            finally
//            {
//                // 确保临时文件被删除
//                if (File.Exists(tempFilePath))
//                {
//                    File.Delete(tempFilePath);
//                }
//            }
//        }
//        public static MemoryStream GenerateSimplePdf(string content, string title, byte[] chartImage = null)
//        {
//            var memoryStream = new MemoryStream();
//            var writer = new PdfWriter(memoryStream);
//            var pdf = new PdfDocument(writer);
//            var document = new Document(pdf);

//            try
//            {
//                // 添加标题
//                document.Add(new Paragraph(title)
//                    .SetFontSize(16)
//                    .SetBold()
//                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

//                // 添加内容
//                foreach (var line in content.Split('\n'))
//                {
//                    document.Add(new Paragraph(line));
//                }

//                // 如果有图表，添加图表
//                if (chartImage != null)
//                {
//                    // 添加图表标题
//                    document.Add(new Paragraph("告警趋势分析")
//                        .SetFontSize(14)
//                        .SetBold()
//                        .SetMarginTop(20)
//                        .SetMarginBottom(10)
//                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

//                    // 添加图表
//                    var image = ImageDataFactory.Create(chartImage);
//                    document.Add(new Image(image)
//                        .SetWidth(500)
//                        .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER));
//                }
//            }
//            finally
//            {
//                document.Close();
//            }

//            return memoryStream;
//        }
//    }
//}