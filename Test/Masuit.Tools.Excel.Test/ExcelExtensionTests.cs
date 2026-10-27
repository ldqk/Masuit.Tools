using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using ExcelEncryptor;
using Masuit.Tools.Excel;
using SkiaSharp;
using System.Data;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace Masuit.Tools.Excel.Test;

public class ExcelExtensionTests
{
    [Fact]
    public void ToExcel_DataTable_ExportsValuesAndColumnFormats()
    {
        var table = new DataTable();
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Amount", typeof(decimal));
        table.Rows.Add("Widget", 12.5m);

        using var stream = table.ToExcel(settings: new ColumnSettings().SetColumnFormat("B", "0.00"));
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Sheet1");

        Assert.Equal("Widget", worksheet.Cell("A2").GetString());
        Assert.Equal(12.5, worksheet.Cell("B2").GetDouble());
        Assert.Equal("0.00", worksheet.Cell("B2").Style.NumberFormat.Format);
    }

    [Fact]
    public void ToExcel_MergeCells_MergesAdjacentEqualNonEmptyValues()
    {
        var table = CreateMergeTable();

        using var stream = table.ToExcel(mergeCells: true);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Sheet1");
        var mergedRanges = worksheet.MergedRanges
            .Select(range => range.RangeAddress.ToString())
            .OrderBy(address => address)
            .ToArray();

        Assert.Equal(new[] { "A2:B2", "A3:A4", "C3:C4" }, mergedRanges);
        Assert.Empty(worksheet.Tables);
    }

    [Fact]
    public void ToExcel_Default_DoesNotMergeAndKeepsTableStyle()
    {
        var table = CreateMergeTable();

        using var stream = table.ToExcel();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Sheet1");

        Assert.Empty(worksheet.MergedRanges);
        Assert.Equal(XLTableTheme.TableStyleLight15, worksheet.Tables.Single().Theme);
    }

    [Fact]
    public void ToExcel_Collection_ExportsAttributesAndMultipleSheets()
    {
        var rows = new[]
        {
            new ReportRow { Name = "Widget", Amount = 12.5m, InternalCode = "hidden" }
        };
        var sheets = new Dictionary<string, IEnumerable<ReportRow>>
        {
            ["Report"] = rows
        };

        using var stream = sheets.ToExcel();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Report");

        Assert.Equal("Name", worksheet.Cell("A1").GetString());
        Assert.Equal("Widget", worksheet.Cell("A2").GetString());
        Assert.Equal(12.5, worksheet.Cell("B2").GetDouble());
        Assert.Equal("#,##0.00", worksheet.Cell("B2").Style.NumberFormat.Format);
        Assert.Equal(2, worksheet.Tables.Single().Fields.Count());
    }

    [Fact]
    public void ToExcel_ImageDictionary_PreservesPictureHyperlink()
    {
        const string url = "https://example.com/image";
        var table = new DataTable();
        table.Columns.Add("Image", typeof(IDictionary<string, Stream>));
        table.Rows.Add(new Dictionary<string, Stream>
        {
            [url] = new MemoryStream(CreatePng())
        });

        using var stream = table.ToExcel();
        using var document = SpreadsheetDocument.Open(stream, false);
        var worksheetPart = document.WorkbookPart!.WorksheetParts.Single();
        var drawingPart = worksheetPart.DrawingsPart!;
        var picture = drawingPart.WorksheetDrawing!.Descendants<Xdr.Picture>().Single();
        var hyperlink = Assert.Single(drawingPart.HyperlinkRelationships);
        var pictureHyperlinkId = picture.NonVisualPictureProperties!
            .NonVisualDrawingProperties!
            .HyperlinkOnClick!
            .Id!
            .Value;

        Assert.Equal(new Uri(url), hyperlink.Uri);
        Assert.Equal(hyperlink.Id, pictureHyperlinkId);
    }

    [Fact]
    public void ToExcel_WithPassword_ProducesDecryptableWorkbook()
    {
        var table = new DataTable();
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add("Widget");
        const string password = "test-password";
        var encryptedPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");

        try
        {
            using (var stream = table.ToExcel(password: password))
            {
                File.WriteAllBytes(encryptedPath, stream.ToArray());
            }

            var decryptedBytes = Encrypt.Decrypt(encryptedPath, password);
            using var decryptedStream = new MemoryStream(decryptedBytes);
            using var workbook = new XLWorkbook(decryptedStream);

            Assert.Equal("Widget", workbook.Worksheet("Sheet1").Cell("A2").GetString());
        }
        finally
        {
            File.Delete(encryptedPath);
        }
    }

    private static DataTable CreateMergeTable()
    {
        var table = new DataTable();
        table.Columns.Add("A", typeof(string));
        table.Columns.Add("B", typeof(string));
        table.Columns.Add("C", typeof(string));
        table.Rows.Add("same", "same", "");
        table.Rows.Add("group", "x", "z");
        table.Rows.Add("group", "y", "z");
        return table;
    }

    private static byte[] CreatePng()
    {
        using var bitmap = new SKBitmap(2, 2);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        return encoded.ToArray();
    }

    private sealed class ReportRow
    {
        public string Name { get; set; } = string.Empty;

        [ColumnFormat("#,##0.00")]
        public decimal Amount { get; set; }

        [ExportIgnore]
        public string InternalCode { get; set; } = string.Empty;
    }
}
