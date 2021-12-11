using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Masuit.Tools.Excel
{
    public static class ExcelExtension
    {
        static ExcelExtension()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// 将内存表自动填充到Excel
        /// </summary>
        /// <param name="sheetTables">sheet名和内存表的映射</param>
        /// <param name="password">密码</param>
        /// <returns>内存流</returns>
        public static MemoryStream ToExcel(this Dictionary<string, DataTable> sheetTables, string password = null, ColumnSettings settings = null)
        {
            using var pkg = new ExcelPackage();
            foreach (var pair in sheetTables)
            {
                pair.Value.TableName = pair.Key;
                CreateWorksheet(pkg, pair.Value, settings);
            }

            return SaveAsStream(pkg, password);
        }

        /// <summary>
        /// 将内存表自动填充到Excel
        /// </summary>
        /// <param name="tables">内存表</param>
        /// <param name="password">密码</param>
        /// <returns>内存流</returns>
        public static MemoryStream ToExcel(this List<DataTable> tables, string password = null, ColumnSettings settings = null)
        {
            using var pkg = new ExcelPackage();
            foreach (var table in tables)
            {
                CreateWorksheet(pkg, table, settings);
            }

            return SaveAsStream(pkg, password);
        }

        /// <summary>
        /// 将内存表自动填充到Excel
        /// </summary>
        /// <param name="table">内存表</param>
        /// <param name="password">密码</param>
        /// <returns>内存流</returns>
        public static MemoryStream ToExcel(this DataTable table, string password = null, ColumnSettings settings = null)
        {
            using var pkg = new ExcelPackage();
            CreateWorksheet(pkg, table, settings);
            return SaveAsStream(pkg, password);
        }

        private static MemoryStream SaveAsStream(ExcelPackage pkg, string password)
        {
            var ms = new MemoryStream();
            if (!string.IsNullOrEmpty(password))
            {
                pkg.SaveAs(ms, password);
            }
            else
            {
                pkg.SaveAs(ms);
            }

            return ms;
        }

        public static void CreateWorksheet(this ExcelPackage pkg, DataTable table, ColumnSettings settings = null)
        {
            if (string.IsNullOrEmpty(table.TableName))
            {
                table.TableName = "Sheet1";
            }

            pkg.Workbook.Worksheets.Add(table.TableName);
            var sheet = pkg.Workbook.Worksheets[table.TableName];

            FillWorksheet(sheet, table, settings);

            //打印方向：纵向
            sheet.PrinterSettings.Orientation = eOrientation.Landscape;

            //集中在一页里打印
            sheet.PrinterSettings.FitToPage = true;

            //使用A4纸
            sheet.PrinterSettings.PaperSize = ePaperSize.A4;
        }

        /// <summary>
        /// 从datatable填充工作簿
        /// </summary>
        /// <param name="sheet">工作簿</param>
        /// <param name="table">数据</param>
        /// <param name="settings">列设置</param>
        /// <param name="startRow">起始行，默认第一行</param>
        /// <param name="startColumn">起始列，默认第一列A列</param>
        public static void FillWorksheet(this ExcelWorksheet sheet, DataTable table, ColumnSettings settings = null, int startRow = 1, int startColumn = 1)
        {
            // 填充表头
            var maxWidth = new int[table.Columns.Count];
            for (var j = 0; j < table.Columns.Count; j++)
            {
                sheet.SetValue(startRow, j + startColumn, table.Columns[j].ColumnName);
                maxWidth[j] = Encoding.UTF8.GetBytes(table.Columns[j].ColumnName).Length;
            }

            sheet.Row(startRow).Style.Font.Bold = true; // 表头设置为粗体
            sheet.Row(startRow).Style.Font.Size = sheet.Row(startRow).Style.Font.Size * 1.11f; // 表头字号放大1.11倍
            sheet.Row(startRow).CustomHeight = true; // 自动调整行高
            sheet.Cells.AutoFitColumns(); // 表头自适应列宽
            sheet.Cells.Style.WrapText = true;
            if (settings != null)
            {
                foreach (var x in settings.ColumnTypes)
                {
                    sheet.Column(x.Key).Style.Numberformat.Format = x.Value;
                }
            }

            // 填充内容
            for (var i = 0; i < table.Rows.Count; i++)
            {
                sheet.Row(i + startRow + 1).CustomHeight = true; // 自动调整行高
                for (var j = 0; j < table.Columns.Count; j++)
                {
                    switch (table.Rows[i][j])
                    {
                        case Stream s:
                            {
                                if (s.Length > 2)
                                {
                                    using var bmp = new Bitmap(s);
                                    bmp.SetResolution(96, 96);
                                    var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), bmp);
                                    picture.SetPosition(i + startRow, 3, j + startColumn - 1, 5); //设置图片显示位置
                                    var percent = 11000f / bmp.Height;
                                    picture.SetSize((int)percent);
                                    sheet.Row(i + startRow + 1).Height = 90;
                                    sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, bmp.Width * percent / 600 > 32 ? bmp.Width * percent / 600 : 32);
                                }
                                sheet.SetValue(i + startRow + 1, j + startColumn, "");

                                break;
                            }

                        case Bitmap bmp:
                            {
                                if (bmp.Width + bmp.Height > 4)
                                {
                                    bmp.SetResolution(96, 96);
                                    var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), bmp);
                                    picture.SetPosition(i + startRow, 3, j + startColumn - 1, 5); //设置图片显示位置
                                    var percent = 11000f / bmp.Height;
                                    picture.SetSize((int)percent);
                                    sheet.Row(i + startRow + 1).Height = 90;
                                    sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, bmp.Width * percent / 600 > 32 ? bmp.Width * percent / 600 : 32);
                                }
                                sheet.SetValue(i + startRow + 1, j + startColumn, "");

                                break;
                            }

                        case IEnumerable<Stream> streams:
                            {
                                double sumWidth = 0;
                                foreach (var stream in streams.Where(stream => stream.Length > 2))
                                {
                                    using var bmp = new Bitmap(stream);
                                    bmp.SetResolution(96, 96);
                                    var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), bmp);
                                    picture.SetPosition(i + startRow, 3, j + startColumn - 1, (int)(5 + sumWidth)); //设置图片显示位置
                                    var percent = 11000f / bmp.Height;
                                    picture.SetSize((int)percent);
                                    sheet.Row(i + startRow + 1).Height = 90;
                                    sumWidth += bmp.Width * 1.0 * percent / 100 + 5;
                                    sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth / 6 > 32 ? sumWidth / 6 : 32);
                                }

                                sheet.SetValue(i + startRow + 1, j + startColumn, "");
                                break;
                            }

                        case IEnumerable<Bitmap> bmps:
                            {
                                double sumWidth = 0;
                                foreach (var bmp in bmps.Where(stream => stream.Width + stream.Height > 4))
                                {
                                    bmp.SetResolution(96, 96);
                                    var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), bmp);
                                    picture.SetPosition(i + startRow, 3, j + startColumn - 1, (int)(5 + sumWidth)); //设置图片显示位置
                                    var percent = 11000f / bmp.Height;
                                    picture.SetSize((int)percent);
                                    sheet.Row(i + startRow + 1).Height = 90;
                                    sumWidth += bmp.Width * 1.0 * percent / 100 + 5;
                                    sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth / 6 > 32 ? sumWidth / 6 : 32);
                                }

                                sheet.SetValue(i + startRow + 1, j + startColumn, "");
                                break;
                            }

                        case IDictionary<string, Stream> dic:
                            {
                                double sumWidth = 0;
                                foreach (var kv in dic.Where(kv => kv.Value.Length > 2))
                                {
                                    using var bmp = new Bitmap(kv.Value);
                                    bmp.SetResolution(96, 96);
                                    var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), bmp, new Uri(kv.Key));
                                    picture.SetPosition(i + startRow, 3, j + startColumn - 1, (int)(5 + sumWidth)); //设置图片显示位置
                                    var percent = 11000f / bmp.Height;
                                    picture.SetSize((int)percent);
                                    sheet.Row(i + startRow + 1).Height = 90;
                                    sumWidth += bmp.Width * 1.0 * percent / 100 + 5;
                                    sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth / 6 > 32 ? sumWidth / 6 : 32);
                                }

                                sheet.SetValue(i + startRow + 1, j + startColumn, "");
                                break;
                            }

                        case IDictionary<string, MemoryStream> dic:
                            {
                                double sumWidth = 0;
                                foreach (var kv in dic.Where(kv => kv.Value.Length > 2))
                                {
                                    using var bmp = new Bitmap(kv.Value);
                                    bmp.SetResolution(96, 96);
                                    var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), bmp, new Uri(kv.Key));
                                    picture.SetPosition(i + startRow, 3, j + startColumn - 1, (int)(5 + sumWidth)); //设置图片显示位置
                                    var percent = 11000f / bmp.Height;
                                    picture.SetSize((int)percent);
                                    sheet.Row(i + startRow + 1).Height = 90;
                                    sumWidth += bmp.Width * 1.0 * percent / 100 + 5;
                                    sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth / 6 > 32 ? sumWidth / 6 : 32);
                                }

                                sheet.SetValue(i + startRow + 1, j + startColumn, "");
                                break;
                            }

                        case IDictionary<string, Bitmap> bmps:
                            {
                                double sumWidth = 0;
                                foreach (var kv in bmps.Where(kv => kv.Value.Width + kv.Value.Height > 4))
                                {
                                    kv.Value.SetResolution(96, 96);
                                    var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), kv.Value, new Uri(kv.Key));
                                    picture.SetPosition(i + startRow, 3, j + startColumn - 1, (int)(5 + sumWidth)); //设置图片显示位置
                                    var percent = 11000f / kv.Value.Height;
                                    picture.SetSize((int)percent);
                                    sheet.Row(i + startRow + 1).Height = 90;
                                    sumWidth += kv.Value.Width * 1.0 * percent / 100 + 5;
                                    sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth / 6 > 32 ? sumWidth / 6 : 32);
                                }

                                sheet.SetValue(i + startRow + 1, j + startColumn, "");
                                break;
                            }

                        default:
                            {
                                sheet.SetValue(i + startRow + 1, j + startColumn, table.Rows[i][j] ?? "");
                                if (table.Rows[i][j] is ValueType)
                                {
                                    sheet.Column(j + startColumn).AutoFit();
                                }
                                else
                                {
                                    // 根据单元格内容长度来自适应调整列宽
                                    sheet.Column(j + startColumn).Width = Math.Max(Encoding.UTF8.GetBytes(table.Rows[i][j].ToString() ?? string.Empty).Length + 2, sheet.Column(j + startColumn).Width);
                                    if (sheet.Column(j + startColumn).Width > 110)
                                    {
                                        sheet.Column(j + startColumn).AutoFit(100, 110);
                                    }
                                }

                                break;
                            }
                    }
                }
            }
        }
    }
}
