using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using OfficeOpenXml.Style;

namespace Masuit.Tools.Excel;

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
    /// <param name="settings">列设置</param>
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

        var sheet = pkg.Workbook.Worksheets.Add(table.TableName);
        sheet.Cells.Style.Font.Name = "微软雅黑";

        FillWorksheet(sheet, table, settings);

        sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

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
        sheet.Row(startRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
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
                                var pictureType = ImageDetector.GetPictureType(s) ?? throw new ArgumentException($"{i}行{j}列图像格式不受支持");
                                var bmp = new ExcelImage(s, pictureType).Bounds;
                                var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), s, pictureType);
                                picture.SetPosition(i + startRow, 3, j + startColumn - 1, 5); //设置图片显示位置
                                var percent = Math.Round(Math.Min(12000f / bmp.Height, 100));
                                picture.SetSize((int)percent);
                                sheet.Row(i + startRow + 1).Height = 90;
                                sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, bmp.Width * percent / 400);
                            }

                            sheet.SetValue(i + startRow + 1, j + startColumn, "");

                            break;
                        }

                    case IEnumerable<Stream> streams:
                        {
                            double sumWidth = 0;
                            foreach (var stream in streams.Where(stream => stream.Length > 2))
                            {
                                var pictureType = ImageDetector.GetPictureType(stream) ?? throw new ArgumentException($"{i}行{j}列图像格式不受支持");
                                var bmp = new ExcelImage(stream, pictureType).Bounds;
                                var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), stream, pictureType);
                                picture.SetPosition(i + startRow, 3, j + startColumn - 1, (int)Math.Ceiling(5 + sumWidth)); //设置图片显示位置
                                var percent = Math.Min(12000f / bmp.Height, 100);
                                picture.SetSize((int)percent);
                                sheet.Row(i + startRow + 1).Height = 90;
                                sumWidth += bmp.Width * percent / 100 + 5;
                                sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth / 5);
                            }

                            sheet.SetValue(i + startRow + 1, j + startColumn, "");
                            break;
                        }

                    case IDictionary<string, Stream> dic:
                        {
                            double sumWidth = 0;
                            foreach (var kv in dic.Where(kv => kv.Value.Length > 2))
                            {
                                var pictureType = ImageDetector.GetPictureType(kv.Value) ?? throw new ArgumentException($"{i}行{j}列图像格式不受支持");
                                var bmp = new ExcelImage(kv.Value, pictureType).Bounds;
                                var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), kv.Value, pictureType, new Uri(kv.Key));
                                picture.SetPosition(i + startRow, 3, j + startColumn - 1, (int)Math.Ceiling(5 + sumWidth)); //设置图片显示位置
                                var percent = Math.Min(12000f / bmp.Height, 100);
                                picture.SetSize((int)percent);
                                sheet.Row(i + startRow + 1).Height = 90;
                                sumWidth += bmp.Width * percent / 100 + 5;
                                sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth / 5);
                            }

                            sheet.SetValue(i + startRow + 1, j + startColumn, "");
                            break;
                        }

                    case IDictionary<string, MemoryStream> dic:
                        {
                            double sumWidth = 0;
                            foreach (var kv in dic.Where(kv => kv.Value.Length > 2))
                            {
                                var pictureType = ImageDetector.GetPictureType(kv.Value) ?? throw new ArgumentException($"{i}行{j}列图像格式不受支持");
                                var bmp = new ExcelImage(kv.Value, pictureType).Bounds;
                                var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), kv.Value, pictureType, new Uri(kv.Key));
                                picture.SetPosition(i + startRow, 3, j + startColumn - 1, (int)Math.Ceiling(5 + sumWidth)); //设置图片显示位置
                                var percent = Math.Min(12000f / bmp.Height, 100);
                                picture.SetSize((int)percent);
                                sheet.Row(i + startRow + 1).Height = 90;
                                sumWidth += bmp.Width * percent / 100 + 5;
                                sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth / 5);
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
                                using var bitmap = new Bitmap(1, 1);
                                using var graphics = Graphics.FromImage(bitmap);
                                using var font = new Font(sheet.Cells[i + startRow + 1, j + startColumn].Style.Font.Name, 1.66f);
                                var measureText = graphics.MeasureString(table.Rows[i][j].ToString(), font);
                                sheet.Column(j + startColumn).Width = Math.Min(110, Math.Max(measureText.Width, sheet.Column(j + startColumn).Width));
                            }

                            break;
                        }
                }
            }
        }
    }

    private static readonly NumberFormater NumberFormater = new("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 1);

    /// <summary>
    /// 获取字母列
    /// </summary>
    /// <param name="sheet"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static ExcelColumn Column(this ExcelWorksheet sheet, string index)
    {
        return sheet.Column((int)NumberFormater.FromString(index));
    }
}
