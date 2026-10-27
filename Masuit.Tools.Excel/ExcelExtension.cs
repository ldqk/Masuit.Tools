using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using ExcelEncryptor;
using Masuit.Tools.Reflection;
using Masuit.Tools.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Spreadsheet;
using Path = System.IO.Path;
using Picture = DocumentFormat.OpenXml.Drawing.Picture;

namespace Masuit.Tools.Excel;

public static class ExcelExtension
{
    private const int ImageMaxHeight = 120;
    private static readonly NumberFormater NumberFormater = new("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 1);

    /// <summary>
    /// 将内存表自动填充到Excel
    /// </summary>
    /// <param name="sheetTables">sheet名和内存表的映射</param>
    /// <param name="password">打开文件的密码</param>
    /// <param name="settings">列设置</param>
    /// <param name="mergeCells">是否合并相邻且值相同的单元格</param>
    /// <returns>内存流</returns>
    public static PooledMemoryStream ToExcel(this Dictionary<string, DataTable> sheetTables, string password = null, ColumnSettings settings = null, bool mergeCells = false)
    {
        using var workbook = new XLWorkbook();
        var pictureHyperlinks = new List<PictureHyperlink>();
        foreach (var pair in sheetTables)
        {
            pair.Value.TableName = pair.Key;
            CreateDataTableWorksheet(workbook, pair.Value, settings, mergeCells, pictureHyperlinks);
        }

        return SaveAsStream(workbook, password, pictureHyperlinks);
    }

    /// <summary>
    /// 将内存表自动填充到Excel
    /// </summary>
    /// <param name="sheetTables">内存表</param>
    /// <param name="password">打开文件的密码</param>
    /// <param name="settings">列设置</param>
    /// <param name="mergeCells">是否合并相邻且值相同的单元格</param>
    /// <returns>内存流</returns>
    public static PooledMemoryStream ToExcel(this IEnumerable<DataTable> sheetTables, string password = null, ColumnSettings settings = null, bool mergeCells = false)
    {
        using var workbook = new XLWorkbook();
        var pictureHyperlinks = new List<PictureHyperlink>();
        foreach (var table in sheetTables)
        {
            CreateDataTableWorksheet(workbook, table, settings, mergeCells, pictureHyperlinks);
        }

        return SaveAsStream(workbook, password, pictureHyperlinks);
    }

    /// <summary>
    /// 将内存表自动填充到Excel
    /// </summary>
    /// <param name="sheetTables">sheet名和内存表的映射</param>
    /// <param name="password">打开文件的密码</param>
    /// <param name="settings">列设置</param>
    /// <param name="mergeCells">是否合并相邻且值相同的单元格</param>
    /// <returns>内存流</returns>
    public static PooledMemoryStream ToExcel<T>(this Dictionary<string, IEnumerable<T>> sheetTables, string password = null, ColumnSettings settings = null, bool mergeCells = false)
    {
        using var workbook = new XLWorkbook();
        var pictureHyperlinks = new List<PictureHyperlink>();
        foreach (var pair in sheetTables)
        {
            CreateObjectWorksheet(workbook, pair, settings, mergeCells, pictureHyperlinks);
        }

        return SaveAsStream(workbook, password, pictureHyperlinks);
    }

    /// <summary>
    /// 将内存表自动填充到Excel
    /// </summary>
    /// <param name="tables">内存表</param>
    /// <param name="password">打开文件的密码</param>
    /// <param name="settings">列设置</param>
    /// <param name="mergeCells">是否合并相邻且值相同的单元格</param>
    /// <returns>内存流</returns>
    public static PooledMemoryStream ToExcel(this List<DataTable> tables, string password = null, ColumnSettings settings = null, bool mergeCells = false)
    {
        using var workbook = new XLWorkbook();
        var pictureHyperlinks = new List<PictureHyperlink>();
        foreach (var table in tables)
        {
            CreateDataTableWorksheet(workbook, table, settings, mergeCells, pictureHyperlinks);
        }

        return SaveAsStream(workbook, password, pictureHyperlinks);
    }

    /// <summary>
    /// 将内存表自动填充到Excel
    /// </summary>
    /// <param name="table">内存表</param>
    /// <param name="password">打开文件的密码</param>
    /// <param name="settings">列设置</param>
    /// <param name="mergeCells">是否合并相邻且值相同的单元格</param>
    /// <returns>内存流</returns>
    public static PooledMemoryStream ToExcel(this DataTable table, string password = null, ColumnSettings settings = null, bool mergeCells = false)
    {
        using var workbook = new XLWorkbook();
        var pictureHyperlinks = new List<PictureHyperlink>();
        CreateDataTableWorksheet(workbook, table, settings, mergeCells, pictureHyperlinks);
        return SaveAsStream(workbook, password, pictureHyperlinks);
    }

    /// <summary>
    /// 将内存表自动填充到Excel
    /// </summary>
    /// <param name="table">内存表</param>
    /// <param name="password">打开文件的密码</param>
    /// <param name="settings">列设置</param>
    /// <param name="mergeCells">是否合并相邻且值相同的单元格</param>
    /// <returns>内存流</returns>
    public static PooledMemoryStream ToExcel<T>(this IEnumerable<T> table, string password = null, ColumnSettings settings = null, bool mergeCells = false)
    {
        using var workbook = new XLWorkbook();
        var pictureHyperlinks = new List<PictureHyperlink>();
        CreateObjectWorksheet(workbook, new KeyValuePair<string, IEnumerable<T>>("Sheet1", table), settings, mergeCells, pictureHyperlinks);
        return SaveAsStream(workbook, password, pictureHyperlinks);
    }

    private static PooledMemoryStream SaveAsStream(XLWorkbook workbook, string password, List<PictureHyperlink> pictureHyperlinks)
    {
        var result = new PooledMemoryStream();
        if (string.IsNullOrEmpty(password))
        {
            using var plainStream = new MemoryStream();
            workbook.SaveAs(plainStream);
            var workbookBytes = AddPictureHyperlinks(plainStream.ToArray(), pictureHyperlinks);
            result.Write(workbookBytes, 0, workbookBytes.Length);
        }
        else
        {
            using var plainStream = new MemoryStream();
            workbook.SaveAs(plainStream);
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");
            try
            {
                var workbookBytes = AddPictureHyperlinks(plainStream.ToArray(), pictureHyperlinks);
                Encrypt.FromBytesToFile(workbookBytes, tempPath, password);
                using var encryptedStream = File.OpenRead(tempPath);
                encryptedStream.CopyTo(result);
            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        result.Position = 0;
        return result;
    }

    /// <summary>
    /// 创建并填充DataTable对应的工作表
    /// </summary>
    /// <param name="workbook">Excel工作簿</param>
    /// <param name="table">内存表</param>
    /// <param name="settings">列设置</param>
    /// <param name="mergeCells">是否合并相邻且值相同的单元格</param>
    public static void CreateWorksheet(this XLWorkbook workbook, DataTable table, ColumnSettings settings = null, bool mergeCells = false)
    {
        CreateDataTableWorksheet(workbook, table, settings, mergeCells, null);
    }

    private static void CreateDataTableWorksheet(XLWorkbook workbook, DataTable table, ColumnSettings settings, bool mergeCells, List<PictureHyperlink> pictureHyperlinks)
    {
        if (string.IsNullOrEmpty(table.TableName))
        {
            table.TableName = "Sheet1";
        }

        var sheet = workbook.Worksheets.Add(table.TableName);
        ConfigureWorksheet(sheet);
        FillDataTable(sheet, table, settings, 1, 1, !mergeCells, pictureHyperlinks);

        if (mergeCells)
        {
            MergeAdjacentCells(sheet);
        }
    }

    /// <summary>
    /// 创建并填充集合对应的工作表
    /// </summary>
    /// <param name="workbook">Excel工作簿</param>
    /// <param name="table">sheet名和内存表的映射</param>
    /// <param name="settings">列设置</param>
    /// <param name="mergeCells">是否合并相邻且值相同的单元格</param>
    public static void CreateWorksheet<T>(this XLWorkbook workbook, KeyValuePair<string, IEnumerable<T>> table, ColumnSettings settings = null, bool mergeCells = false)
    {
        CreateObjectWorksheet(workbook, table, settings, mergeCells, null);
    }

    private static void CreateObjectWorksheet<T>(XLWorkbook workbook, KeyValuePair<string, IEnumerable<T>> table, ColumnSettings settings, bool mergeCells, List<PictureHyperlink> pictureHyperlinks)
    {
        var sheet = workbook.Worksheets.Add(table.Key);
        ConfigureWorksheet(sheet);
        FillCollection(sheet, table.Value, settings, 1, 1, !mergeCells, pictureHyperlinks);

        if (mergeCells)
        {
            MergeAdjacentCells(sheet);
        }
    }

    private static void ConfigureWorksheet(IXLWorksheet sheet)
    {
        sheet.Style.Font.FontName = "微软雅黑";
        sheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        sheet.PageSetup.FitToPages(1, 0);
        sheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
    }

    /// <summary>
    /// 从DataTable填充工作表
    /// </summary>
    public static void FillWorksheet(this IXLWorksheet sheet, DataTable table, ColumnSettings settings = null, int startRow = 1, int startColumn = 1)
    {
        FillDataTable(sheet, table, settings, startRow, startColumn, true, null);
    }

    private static void FillDataTable(IXLWorksheet sheet, DataTable table, ColumnSettings settings, int startRow, int startColumn, bool createTable, List<PictureHyperlink> pictureHyperlinks)
    {
        if (table.Rows.Count == 0 || table.Columns.Count == 0)
        {
            return;
        }

        var imageColumns = table.Columns.Cast<DataColumn>().Select((column, index) => (column, index)).Where(item => IsPictureType(item.column.DataType) || table.Rows.Cast<DataRow>().Any(row => IsPictureValue(row[item.index]))).Select(item => startColumn + item.index).ToHashSet();
        var hasImages = imageColumns.Count > 0;
        var lastRow = startRow + table.Rows.Count;
        var lastColumn = startColumn + table.Columns.Count - 1;

        for (var columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
        {
            SetCellValue(sheet.Cell(startRow, startColumn + columnIndex), table.Columns[columnIndex].ColumnName);
        }

        StyleHeader(sheet, startRow, startColumn, lastColumn);

        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            var excelRow = startRow + rowIndex + 1;
            for (var columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
            {
                var excelColumn = startColumn + columnIndex;
                var value = table.Rows[rowIndex][columnIndex];
                if (IsPictureValue(value))
                {
                    AddPictures(sheet, value, excelRow, excelColumn, $"{excelRow}行{columnIndex + 1}列", pictureHyperlinks);
                    sheet.Cell(excelRow, excelColumn).Value = string.Empty;
                }
                else
                {
                    SetCellValue(sheet.Cell(excelRow, excelColumn), value);
                }
            }
        }

        sheet.Style.Alignment.WrapText = true;
        ApplyColumnSettings(sheet, settings);

        if (createTable && !hasImages && HasValidHeaders(sheet, startRow, startColumn, lastColumn))
        {
            CreateStyledTable(sheet, startRow, startColumn, lastRow, lastColumn);
        }

        AdjustColumns(sheet, startRow, lastRow, startColumn, lastColumn, Enumerable.Range(startColumn, table.Columns.Count).Where(column => !imageColumns.Contains(column)));
    }

    /// <summary>
    /// 从集合填充工作表
    /// </summary>
    public static void FillWorksheet<T>(this IXLWorksheet sheet, IEnumerable<T> source, ColumnSettings settings = null, int startRow = 1, int startColumn = 1)
    {
        FillCollection(sheet, source, settings, startRow, startColumn, true, null);
    }

    private static void FillCollection<T>(IXLWorksheet sheet, IEnumerable<T> source, ColumnSettings settings, int startRow, int startColumn, bool createTable, List<PictureHyperlink> pictureHyperlinks)
    {
        var properties = typeof(T).GetProperties().Where(property => property.GetIndexParameters().Length == 0 && property.GetAttribute<ExportIgnoreAttribute>() == null && !HasEpplusIgnoreAttribute(property)).ToList();
        var items = source as ICollection<T> ?? source.ToList();
        if (items.Count == 0 || properties.Count == 0)
        {
            return;
        }

        var imageColumns = properties.Select((property, index) => (property, index)).Where(entry => IsPictureType(entry.property.PropertyType) || items.Any(item => item != null && IsPictureValue(entry.property.GetValue(item)))).Select(entry => startColumn + entry.index).ToHashSet();
        var hasImages = imageColumns.Count > 0;
        var lastRow = startRow + items.Count;
        var lastColumn = startColumn + properties.Count - 1;

        for (var columnIndex = 0; columnIndex < properties.Count; columnIndex++)
        {
            var property = properties[columnIndex];
            SetCellValue(sheet.Cell(startRow, startColumn + columnIndex), property.Name);
            if (property.GetAttribute<ColumnFormatAttribute>() is { } format)
            {
                sheet.Column(startColumn + columnIndex).Style.NumberFormat.Format = format.FormatString;
            }
        }

        StyleHeader(sheet, startRow, startColumn, lastColumn);

        var rowIndex = 0;
        foreach (var item in items)
        {
            var excelRow = startRow + rowIndex + 1;
            for (var columnIndex = 0; columnIndex < properties.Count; columnIndex++)
            {
                var excelColumn = startColumn + columnIndex;
                var value = item == null ? null : properties[columnIndex].GetValue(item);
                if (IsPictureValue(value))
                {
                    AddPictures(sheet, value, excelRow, excelColumn, $"{excelRow}行{columnIndex + 1}列", pictureHyperlinks);
                    sheet.Cell(excelRow, excelColumn).Value = string.Empty;
                }
                else
                {
                    SetCellValue(sheet.Cell(excelRow, excelColumn), value);
                }
            }

            rowIndex++;
        }

        sheet.Style.Alignment.WrapText = true;
        ApplyColumnSettings(sheet, settings);

        if (createTable && !hasImages && HasValidHeaders(sheet, startRow, startColumn, lastColumn))
        {
            CreateStyledTable(sheet, startRow, startColumn, lastRow, lastColumn);
        }

        AdjustColumns(sheet, startRow, lastRow, startColumn, lastColumn, Enumerable.Range(startColumn, properties.Count).Where(column => !imageColumns.Contains(column)));
    }

    private static void StyleHeader(IXLWorksheet sheet, int row, int startColumn, int endColumn)
    {
        var header = sheet.Range(row, startColumn, row, endColumn);
        header.Style.Font.Bold = true;
        header.Style.Font.FontSize = header.Style.Font.FontSize * 1.11;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private static void ApplyColumnSettings(IXLWorksheet sheet, ColumnSettings settings)
    {
        if (settings == null)
        {
            return;
        }

        foreach (var column in settings.ColumnTypes)
        {
            sheet.Column(column.Key).Style.NumberFormat.Format = column.Value;
        }
    }

    private static bool HasValidHeaders(IXLWorksheet sheet, int row, int startColumn, int endColumn)
    {
        var headers = Enumerable.Range(startColumn, endColumn - startColumn + 1).Select(column => sheet.Cell(row, column).GetString()).ToList();
        return headers.All(header => !string.IsNullOrWhiteSpace(header)) && headers.Distinct(StringComparer.OrdinalIgnoreCase).Count() == headers.Count;
    }

    private static void CreateStyledTable(IXLWorksheet sheet, int startRow, int startColumn, int lastRow, int lastColumn)
    {
        var table = sheet.Range(startRow, startColumn, lastRow, lastColumn).CreateTable();
        table.Theme = XLTableTheme.TableStyleLight15;
    }

    private static void AdjustColumns(IXLWorksheet sheet, int startRow, int lastRow, int startColumn, int lastColumn, IEnumerable<int> columns)
    {
        foreach (var column in columns.Distinct())
        {
            sheet.Column(column).AdjustToContents(startRow, lastRow, 12, 90);
        }
    }

    private static void MergeAdjacentCells(IXLWorksheet sheet)
    {
        var usedRange = sheet.RangeUsed();
        if (usedRange == null)
        {
            return;
        }

        var startRow = usedRange.RangeAddress.FirstAddress.RowNumber;
        var endRow = usedRange.RangeAddress.LastAddress.RowNumber;
        var startColumn = usedRange.RangeAddress.FirstAddress.ColumnNumber;
        var endColumn = usedRange.RangeAddress.LastAddress.ColumnNumber;
        var merged = new bool[endRow - startRow + 1, endColumn - startColumn + 1];

        for (var row = startRow; row <= endRow; row++)
        {
            for (var column = startColumn; column <= endColumn;)
            {
                var cell = sheet.Cell(row, column);
                var firstValue = cell.Value;
                if (merged[row - startRow, column - startColumn] || cell.IsEmpty())
                {
                    column++;
                    continue;
                }

                var lastColumn = column;
                while (lastColumn < endColumn && !merged[row - startRow, lastColumn + 1 - startColumn] && Equals(firstValue, sheet.Cell(row, lastColumn + 1).Value))
                {
                    lastColumn++;
                }

                if (lastColumn > column)
                {
                    sheet.Range(row, column, row, lastColumn).Merge();
                    for (var mergedColumn = column; mergedColumn <= lastColumn; mergedColumn++)
                    {
                        merged[row - startRow, mergedColumn - startColumn] = true;
                    }
                }

                column = lastColumn + 1;
            }
        }

        for (var column = startColumn; column <= endColumn; column++)
        {
            for (var row = startRow; row <= endRow;)
            {
                var cell = sheet.Cell(row, column);
                var firstValue = cell.Value;
                if (merged[row - startRow, column - startColumn] || cell.IsEmpty())
                {
                    row++;
                    continue;
                }

                var lastRow = row;
                while (lastRow < endRow && !merged[lastRow + 1 - startRow, column - startColumn] && Equals(firstValue, sheet.Cell(lastRow + 1, column).Value))
                {
                    lastRow++;
                }

                if (lastRow > row)
                {
                    sheet.Range(row, column, lastRow, column).Merge();
                    for (var mergedRow = row; mergedRow <= lastRow; mergedRow++)
                    {
                        merged[mergedRow - startRow, column - startColumn] = true;
                    }
                }

                row = lastRow + 1;
            }
        }
    }

    private static void SetCellValue(IXLCell cell, object value)
    {
        switch (value)
        {
            case null:
            case DBNull:
                cell.Value = string.Empty;
                break;
            case string text:
                cell.Value = text;
                break;
            case DateTime dateTime:
                cell.Value = dateTime;
                break;
            case DateTimeOffset dateTimeOffset:
                cell.Value = dateTimeOffset.DateTime;
                break;
            case TimeSpan timeSpan:
                cell.Value = timeSpan;
                break;
            case bool boolean:
                cell.Value = boolean;
                break;
            case Enum enumValue:
                cell.Value = Convert.ToDouble(enumValue, CultureInfo.InvariantCulture);
                break;
            case byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal:
                cell.Value = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                break;
            default:
                cell.Value = Convert.ToString(value, CultureInfo.CurrentCulture);
                break;
        }
    }

    private static bool IsPictureType(Type type)
    {
        if (typeof(Stream).IsAssignableFrom(type))
        {
            return true;
        }

        return type.GetInterfaces().Append(type).Any(candidate => candidate.IsGenericType && ((candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>) && typeof(Stream).IsAssignableFrom(candidate.GenericTypeArguments[0])) || (candidate.GetGenericTypeDefinition() == typeof(IDictionary<,>) && candidate.GenericTypeArguments[0] == typeof(string) && typeof(Stream).IsAssignableFrom(candidate.GenericTypeArguments[1]))));
    }

    private static bool IsPictureValue(object value)
    {
        return value is Stream || (value is IDictionary dictionary && dictionary.Values.Cast<object>().Any(item => item is Stream)) || (value is IEnumerable enumerable and not string && enumerable.Cast<object>().Any(item => item is Stream));
    }

    private static void AddPictures(IXLWorksheet sheet, object value, int row, int column, string location, List<PictureHyperlink> pictureHyperlinks)
    {
        var offsetX = 0;
        var pictureIndex = 0;
        var cell = sheet.Cell(row, column);

        foreach (var content in GetPictures(value))
        {
            var bytes = ReadStream(content.Stream);
            if (bytes.Length <= 2)
            {
                continue;
            }

            using var decodeStream = new MemoryStream(bytes);
            using var bitmap = SkiaSharp.SKBitmap.Decode(decodeStream);
            if (bitmap == null || bitmap.Width <= 0 || bitmap.Height <= 0)
            {
                throw new ArgumentException($"{location}图像格式不受支持");
            }

            var scale = Math.Min((double) ImageMaxHeight / bitmap.Height, 1d);
            var width = Math.Max(1, (int) Math.Round(bitmap.Width * scale));
            var height = Math.Max(1, (int) Math.Round(bitmap.Height * scale));

            using var pictureStream = new MemoryStream(bytes);
            var picture = sheet.AddPicture(pictureStream).MoveTo(cell, offsetX, 5);
            picture.Width = width;
            picture.Height = height;

            if (content.Hyperlink != null)
            {
                cell.SetHyperlink(new XLHyperlink(content.Hyperlink));
            }

            if (content.Hyperlink != null)
            {
                pictureHyperlinks?.Add(new PictureHyperlink(sheet.Name, row - 1, column - 1, content.Hyperlink));
            }

            offsetX += width;
            pictureIndex++;
        }

        if (pictureIndex > 0)
        {
            sheet.Row(row).Height = 90;
            sheet.Column(column).Width = Math.Max(sheet.Column(column).Width, offsetX / 6.4);
        }
    }

    private static IEnumerable<PictureContent> GetPictures(object value)
    {
        if (value is Stream stream)
        {
            yield return new PictureContent(stream, null);
            yield break;
        }

        if (value is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Value is Stream image)
                {
                    yield return new PictureContent(image, entry.Key is string url ? new Uri(url) : null);
                }
            }

            yield break;
        }

        if (value is IEnumerable enumerable && value is not string)
        {
            foreach (var item in enumerable)
            {
                if (item is Stream image)
                {
                    yield return new PictureContent(image, null);
                }
            }
        }
    }

    private static byte[] ReadStream(Stream stream)
    {
        var originalPosition = stream.CanSeek ? stream.Position : (long?) null;
        try
        {
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            return buffer.ToArray();
        }
        finally
        {
            if (originalPosition.HasValue)
            {
                stream.Position = originalPosition.Value;
            }
        }
    }

    private static byte[] AddPictureHyperlinks(byte[] workbookBytes, List<PictureHyperlink> pictureHyperlinks)
    {
        if (pictureHyperlinks.Count == 0)
        {
            return workbookBytes;
        }

        // ClosedXML supports cell hyperlinks but not picture hyperlinks, so add drawing relationships after serialization.
        var linksByCell = pictureHyperlinks.GroupBy(link => (link.WorksheetName, link.Row, link.Column)).ToDictionary(group => group.Key, group => new Queue<Uri>(group.Select(link => link.Hyperlink)));

        using var stream = new MemoryStream();
        stream.Write(workbookBytes, 0, workbookBytes.Length);
        stream.Position = 0;

        using (var document = SpreadsheetDocument.Open(stream, true))
        {
            var workbookPart = document.WorkbookPart;
            foreach (var sheet in workbookPart.Workbook.Sheets.Elements<Sheet>())
            {
                var worksheetPart = (WorksheetPart) workbookPart.GetPartById(sheet.Id.Value);
                var drawingPart = worksheetPart.DrawingsPart;
                var drawing = drawingPart?.WorksheetDrawing;
                if (drawingPart == null || drawing == null)
                {
                    continue;
                }

                foreach (var anchor in drawing.ChildElements)
                {
                    var marker = anchor switch
                    {
                        OneCellAnchor oneCellAnchor => oneCellAnchor.FromMarker,
                        TwoCellAnchor twoCellAnchor => twoCellAnchor.FromMarker,
                        _ => null
                    };
                    var picture = anchor.GetFirstChild<Picture>();
                    if (marker == null || picture?.NonVisualPictureProperties?.NonVisualDrawingProperties == null)
                    {
                        continue;
                    }

                    var key = (sheet.Name.Value, int.Parse(marker.RowId.Text, CultureInfo.InvariantCulture), int.Parse(marker.ColumnId.Text, CultureInfo.InvariantCulture));
                    if (!linksByCell.TryGetValue(key, out var links) || links.Count == 0)
                    {
                        continue;
                    }

                    var relationship = drawingPart.AddHyperlinkRelationship(links.Dequeue(), true);
                    picture.NonVisualPictureProperties.NonVisualDrawingProperties.HyperlinkOnClick = new HyperlinkOnClick
                    {
                        Id = relationship.Id
                    };
                }

                drawing.Save();
            }
        }

        return stream.ToArray();
    }

    private static bool HasEpplusIgnoreAttribute(System.Reflection.PropertyInfo property)
    {
        return property.GetCustomAttributes(false).Any(attribute => attribute.GetType().FullName == "OfficeOpenXml.Attributes.EpplusIgnore");
    }

    /// <summary>
    /// 获取字母列
    /// </summary>
    public static IXLColumn Column(this IXLWorksheet sheet, string index)
    {
        return sheet.Column((int) NumberFormater.FromString(index));
    }

    /// <summary>
    /// 获取单元格值
    /// </summary>
    public static T GetValue<T>(this IXLCell cell, T defaultValue = default)
    {
        return cell.IsEmpty() ? defaultValue : cell.GetValue<T>();
    }

    private sealed class PictureContent
    {
        public PictureContent(Stream stream, Uri hyperlink)
        {
            Stream = stream;
            Hyperlink = hyperlink;
        }

        public Stream Stream { get; }
        public Uri Hyperlink { get; }
    }

    private sealed class PictureHyperlink
    {
        public PictureHyperlink(string worksheetName, int row, int column, Uri hyperlink)
        {
            WorksheetName = worksheetName;
            Row = row;
            Column = column;
            Hyperlink = hyperlink;
        }

        public string WorksheetName { get; }
        public int Row { get; }
        public int Column { get; }
        public Uri Hyperlink { get; }
    }
}