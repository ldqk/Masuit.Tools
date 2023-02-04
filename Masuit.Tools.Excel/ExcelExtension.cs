using Masuit.Tools.Systems;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using SixLabors.Fonts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Masuit.Tools.Excel;

public static class ExcelExtension
{
	static ExcelExtension()
	{
		ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
	}

#if NET5_0_OR_GREATER
	[System.Runtime.CompilerServices.ModuleInitializer]
	internal static void Init()
	{
		ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
	}
#endif

	/// <summary>
	/// 将内存表自动填充到Excel
	/// </summary>
	/// <param name="sheetTables">sheet名和内存表的映射</param>
	/// <param name="password">密码</param>
	/// <param name="settings">列设置</param>
	/// <returns>内存流</returns>
	public static PooledMemoryStream ToExcel(this Dictionary<string, DataTable> sheetTables, string password = null, ColumnSettings settings = null)
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
	/// <param name="sheetTables">sheet名和内存表的映射</param>
	/// <param name="password">密码</param>
	/// <param name="settings">列设置</param>
	/// <returns>内存流</returns>
	public static PooledMemoryStream ToExcel<T>(this Dictionary<string, IEnumerable<T>> sheetTables, string password = null, ColumnSettings settings = null)
	{
		using var pkg = new ExcelPackage();
		foreach (var pair in sheetTables)
		{
			CreateWorksheet(pkg, pair, settings);
		}

		return SaveAsStream(pkg, password);
	}

	/// <summary>
	/// 将内存表自动填充到Excel
	/// </summary>
	/// <param name="tables">内存表</param>
	/// <param name="password">密码</param>
	/// <returns>内存流</returns>
	public static PooledMemoryStream ToExcel(this List<DataTable> tables, string password = null, ColumnSettings settings = null)
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
	public static PooledMemoryStream ToExcel(this DataTable table, string password = null, ColumnSettings settings = null)
	{
		using var pkg = new ExcelPackage();
		CreateWorksheet(pkg, table, settings);
		return SaveAsStream(pkg, password);
	}

	/// <summary>
	/// 将内存表自动填充到Excel
	/// </summary>
	/// <param name="table">内存表</param>
	/// <param name="password">密码</param>
	/// <returns>内存流</returns>
	public static PooledMemoryStream ToExcel<T>(this IEnumerable<T> table, string password = null, ColumnSettings settings = null)
	{
		using var pkg = new ExcelPackage();
		CreateWorksheet(pkg, new KeyValuePair<string, IEnumerable<T>>("Sheet1", table), settings);
		return SaveAsStream(pkg, password);
	}

	private static PooledMemoryStream SaveAsStream(ExcelPackage pkg, string password)
	{
		var ms = new PooledMemoryStream();
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

	public static void CreateWorksheet<T>(this ExcelPackage pkg, KeyValuePair<string, IEnumerable<T>> table, ColumnSettings settings = null)
	{
		var sheet = pkg.Workbook.Worksheets.Add(table.Key);
		sheet.Cells.Style.Font.Name = "微软雅黑";

		FillWorksheet(sheet, table.Value, settings);

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
		var hasPicColumn = false;
		if (table.Rows.Count > 0)
		{
			for (int i = 0; i < table.Columns.Count; i++)
			{
				switch (table.Rows[0][i])
				{
					case Stream:
					case IEnumerable<Stream>:
					case IDictionary<string, Stream>:
					case IDictionary<string, MemoryStream>:
						hasPicColumn = true;
						break;
				}
			}

			if (hasPicColumn)
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
				if (settings != null)
				{
					foreach (var x in settings.ColumnTypes)
					{
						sheet.Column(x.Key).Style.Numberformat.Format = x.Value;
					}
				}
				sheet.Cells.AutoFitColumns(); // 表头自适应列宽

				// 填充内容
				for (int i = 0; i < table.Rows.Count; i++)
				{
					sheet.Row(i + startRow + 1).CustomHeight = true; // 自动调整行高
					for (int j = 0; j < table.Columns.Count; j++)
					{
						switch (table.Rows[i][j])
						{
							case Stream s:
								{
									if (s.Length > 2)
									{
										try
										{
											var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), s);
											picture.SetPosition(i + startRow, 3, j + startColumn - 1, 5); //设置图片显示位置
											var percent = Math.Round(Math.Min(120f / picture.Image.Bounds.Height * picture.Image.Bounds.VerticalResolution, 100));
											sheet.Row(i + startRow + 1).Height = 90;
											sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent / 6.4);
											picture.SetSize((int)percent);
										}
										catch
										{
											throw new ArgumentException($"{i + startRow}行{j}列图像格式不受支持");
										}
									}

									sheet.SetValue(i + startRow + 1, j + startColumn, "");

									break;
								}

							case IEnumerable<Stream> streams:
								{
									double sumWidth = 0;
									int index = 0;
									foreach (var stream in streams.Where(stream => stream.Length > 2))
									{
										try
										{
											var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), stream);
											var percent = Math.Round(Math.Min(120f / picture.Image.Bounds.Height * picture.Image.Bounds.VerticalResolution, 100));
											picture.SetPosition(i + startRow, 3, j + startColumn - 1, (int)Math.Ceiling(picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent * index++)); //设置图片显示位置
											sheet.Row(i + startRow + 1).Height = 90;
											sumWidth += picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent / 6.4;
											sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth);
											picture.SetSize((int)percent);
										}
										catch
										{
											throw new ArgumentException($"{i + startRow}行{j}列第{index}张图像格式不受支持");
										}
									}

									sheet.SetValue(i + startRow + 1, j + startColumn, "");
									break;
								}

							case IDictionary<string, Stream> dic:
								{
									double sumWidth = 0;
									int index = 0;
									foreach (var kv in dic.Where(kv => kv.Value.Length > 2))
									{
										try
										{
											var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), kv.Value, new Uri(kv.Key));
											var percent = Math.Round(Math.Min(120f / picture.Image.Bounds.Height * picture.Image.Bounds.VerticalResolution, 100));
											picture.SetPosition(i + startRow, 3, j + startColumn - 1, (int)Math.Ceiling(picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent * index++)); //设置图片显示位置
											sheet.Row(i + startRow + 1).Height = 90;
											sumWidth += picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent / 6.4;
											sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth);
											picture.SetSize((int)percent);
										}
										catch
										{
											throw new ArgumentException($"{i + startRow}行{j}列第{index}张图像格式不受支持，图片链接：{kv.Key}");
										}
									}

									sheet.SetValue(i + startRow + 1, j + startColumn, "");
									break;
								}

							case IDictionary<string, MemoryStream> dic:
								{
									double sumWidth = 0;
									int index = 0;
									foreach (var kv in dic.Where(kv => kv.Value.Length > 2))
									{
										try
										{
											var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), kv.Value, new Uri(kv.Key));
											var percent = Math.Round(Math.Min(120f / picture.Image.Bounds.Height * picture.Image.Bounds.VerticalResolution, 100));
											picture.SetPosition(i + startRow, 3, j + startColumn - 1, (int)Math.Ceiling(picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent * index++)); //设置图片显示位置
											sheet.Row(i + startRow + 1).Height = 90;
											sumWidth += picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent / 6.4;
											sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth);
											picture.SetSize((int)percent);
										}
										catch
										{
											throw new ArgumentException($"{i + startRow}行{j}列第{index}张图像格式不受支持，图片链接：{kv.Key}");
										}
									}

									sheet.SetValue(i + startRow + 1, j + startColumn, "");
									break;
								}

							case IDictionary<string, PooledMemoryStream> dic:
								{
									double sumWidth = 0;
									int index = 0;
									foreach (var kv in dic.Where(kv => kv.Value.Length > 2))
									{
										try
										{
											var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), kv.Value, new Uri(kv.Key));
											var percent = Math.Round(Math.Min(120f / picture.Image.Bounds.Height * picture.Image.Bounds.VerticalResolution, 100));
											picture.SetPosition(i + startRow, 3, j + startColumn - 1, (int)Math.Ceiling(picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent * index++)); //设置图片显示位置
											sheet.Row(i + startRow + 1).Height = 90;
											sumWidth += picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent / 6.4;
											sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth);
											picture.SetSize((int)percent);
										}
										catch
										{
											throw new ArgumentException($"{i + startRow}行{j}列第{index}张图像格式不受支持，图片链接：{kv.Key}");
										}
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
										var fontFamily = SystemFonts.Families.FirstOrDefault(f => f.Name == sheet.Cells[i + startRow + 1, j + startColumn].Style.Font.Name);
										int measureSize = 1;
										if (fontFamily == default)
										{
											fontFamily = SystemFonts.Families.FirstOrDefault();
											measureSize++;
										}

										var width = TextMeasurer.Measure(table.Rows[i][j].ToString(), new TextOptions(fontFamily.CreateFont(measureSize))).Width;
										sheet.Column(j + startColumn).Width = Math.Min(110, Math.Max(width, sheet.Column(j + startColumn).Width));
									}

									break;
								}
						}
					}
				}

				sheet.Cells.Style.WrapText = true;
			}
			else
			{
				sheet.Cells[startRow, startColumn].LoadFromDataTable(table, true, TableStyles.Light15).AutoFitColumns(12, 90);
				sheet.Cells.Style.WrapText = true;
			}
		}
	}

	/// <summary>
	/// 从datatable填充工作簿
	/// </summary>
	/// <param name="sheet">工作簿</param>
	/// <param name="table">数据</param>
	/// <param name="settings">列设置</param>
	/// <param name="startRow">起始行，默认第一行</param>
	/// <param name="startColumn">起始列，默认第一列A列</param>
	public static void FillWorksheet<T>(this ExcelWorksheet sheet, IEnumerable<T> source, ColumnSettings settings = null, int startRow = 1, int startColumn = 1)
	{
		var hasPicColumn = false;
		var properties = typeof(T).GetProperties();
		if (source is ICollection<T> table)
		{
		}
		else
		{
			table = source.ToList();
		}

		if (table.Any())
		{
			if (properties.Any(t => t.PropertyType.IsSubclassOf(typeof(Stream)) || typeof(IEnumerable<Stream>).IsAssignableFrom(t.PropertyType) || (typeof(IDictionary).IsAssignableFrom(t.PropertyType) && t.PropertyType.GenericTypeArguments[1].IsSubclassOf(typeof(Stream)))))
			{
				hasPicColumn = true;
			}

			if (hasPicColumn)
			{
				// 填充表头
				var maxWidth = new int[properties.Length];
				for (var j = 0; j < properties.Length; j++)
				{
					sheet.SetValue(startRow, j + startColumn, properties[j].Name);
					maxWidth[j] = Encoding.UTF8.GetBytes(properties[j].Name).Length;
				}

				sheet.Row(startRow).Style.Font.Bold = true; // 表头设置为粗体
				sheet.Row(startRow).Style.Font.Size = sheet.Row(startRow).Style.Font.Size * 1.11f; // 表头字号放大1.11倍
				sheet.Row(startRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				sheet.Row(startRow).CustomHeight = true; // 自动调整行高
				if (settings != null)
				{
					foreach (var x in settings.ColumnTypes)
					{
						sheet.Column(x.Key).Style.Numberformat.Format = x.Value;
					}
				}
				sheet.Cells.AutoFitColumns(); // 表头自适应列宽

				// 填充内容
				int current = 0;
				foreach (var item in table)
				{
					sheet.Row(current + startRow + 1).CustomHeight = true; // 自动调整行高
					for (int j = 0; j < properties.Length; j++)
					{
						switch (properties[j].GetValue(item))
						{
							case Stream s:
								{
									if (s.Length > 2)
									{
										try
										{
											var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), s);
											picture.SetPosition(current + startRow, 3, j + startColumn - 1, 5); //设置图片显示位置
											var percent = Math.Round(Math.Min(120f / picture.Image.Bounds.Height * picture.Image.Bounds.VerticalResolution, 100));
											sheet.Row(current + startRow + 1).Height = 90;
											sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent / 6.4);
											picture.SetSize((int)percent);
										}
										catch
										{
											throw new ArgumentException($"{current + startRow}行{j}列图像格式不受支持");
										}
									}

									sheet.SetValue(current + startRow + 1, j + startColumn, "");

									break;
								}

							case IEnumerable<Stream> streams:
								{
									double sumWidth = 0;
									int index = 0;
									foreach (var stream in streams.Where(stream => stream.Length > 2))
									{
										try
										{
											var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), stream);
											var percent = Math.Round(Math.Min(120f / picture.Image.Bounds.Height * picture.Image.Bounds.VerticalResolution, 100));
											picture.SetPosition(current + startRow, 3, j + startColumn - 1, (int)Math.Ceiling(picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent * index++)); //设置图片显示位置
											sheet.Row(current + startRow + 1).Height = 90;
											sumWidth += picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent / 6.4;
											sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth);
											picture.SetSize((int)percent);
										}
										catch
										{
											throw new ArgumentException($"{current + startRow}行{j}列第{index}张图像格式不受支持");
										}
									}

									sheet.SetValue(current + startRow + 1, j + startColumn, "");
									break;
								}

							case IDictionary<string, Stream> dic:
								{
									double sumWidth = 0;
									int index = 0;
									foreach (var kv in dic.Where(kv => kv.Value.Length > 2))
									{
										try
										{
											var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), kv.Value, new Uri(kv.Key));
											var percent = Math.Round(Math.Min(120f / picture.Image.Bounds.Height * picture.Image.Bounds.VerticalResolution, 100));
											picture.SetPosition(current + startRow, 3, j + startColumn - 1, (int)Math.Ceiling(picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent * index++)); //设置图片显示位置
											sheet.Row(current + startRow + 1).Height = 90;
											sumWidth += picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent / 6.4;
											sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth);
											picture.SetSize((int)percent);
										}
										catch
										{
											throw new ArgumentException($"{current + startRow}行{j}列第{index}张图像格式不受支持，图片链接：{kv.Key}");
										}
									}

									sheet.SetValue(current + startRow + 1, j + startColumn, "");
									break;
								}

							case IDictionary<string, MemoryStream> dic:
								{
									double sumWidth = 0;
									int index = 0;
									foreach (var kv in dic.Where(kv => kv.Value.Length > 2))
									{
										try
										{
											var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), kv.Value, new Uri(kv.Key));
											var percent = Math.Round(Math.Min(120f / picture.Image.Bounds.Height * picture.Image.Bounds.VerticalResolution, 100));
											picture.SetPosition(current + startRow, 3, j + startColumn - 1, (int)Math.Ceiling(picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent * index++)); //设置图片显示位置
											sheet.Row(current + startRow + 1).Height = 90;
											sumWidth += picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent / 6.4;
											sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth);
											picture.SetSize((int)percent);
										}
										catch
										{
											throw new ArgumentException($"{current + startRow}行{j}列第{index}张图像格式不受支持，图片链接： {kv.Key}");
										}
									}

									sheet.SetValue(current + startRow + 1, j + startColumn, "");
									break;
								}

							case IDictionary<string, PooledMemoryStream> dic:
								{
									double sumWidth = 0;
									int index = 0;
									foreach (var kv in dic.Where(kv => kv.Value.Length > 2))
									{
										try
										{
											var picture = sheet.Drawings.AddPicture(Guid.NewGuid().ToString(), kv.Value, new Uri(kv.Key));
											var percent = Math.Round(Math.Min(120f / picture.Image.Bounds.Height * picture.Image.Bounds.VerticalResolution, 100));
											picture.SetPosition(current + startRow, 3, j + startColumn - 1, (int)Math.Ceiling(picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent * index++)); //设置图片显示位置
											sheet.Row(current + startRow + 1).Height = 90;
											sumWidth += picture.Image.Bounds.Width / picture.Image.Bounds.HorizontalResolution * percent / 6.4;
											sheet.Column(j + startColumn).Width = Math.Max(sheet.Column(j + startColumn).Width, sumWidth);
											picture.SetSize((int)percent);
										}
										catch
										{
											throw new ArgumentException($"{current + startRow}行{j}列第{index}张图像格式不受支持，图片链接： {kv.Key}");
										}
									}

									sheet.SetValue(current + startRow + 1, j + startColumn, "");
									break;
								}

							default:
								{
									sheet.SetValue(current + startRow + 1, j + startColumn, properties[j].GetValue(item) ?? "");
									if (properties[j].GetValue(item) is ValueType)
									{
										sheet.Column(j + startColumn).AutoFit();
									}
									else
									{
										// 根据单元格内容长度来自适应调整列宽
										var fontFamily = SystemFonts.Families.FirstOrDefault(f => f.Name == sheet.Cells[current + startRow + 1, j + startColumn].Style.Font.Name);
										int measureSize = 1;
										if (fontFamily == default)
										{
											fontFamily = SystemFonts.Families.FirstOrDefault();
											measureSize++;
										}

										var width = TextMeasurer.Measure(properties[j].GetValue(item).ToString(), new TextOptions(fontFamily.CreateFont(measureSize))).Width;
										sheet.Column(j + startColumn).Width = Math.Min(110, Math.Max(width, sheet.Column(j + startColumn).Width));
									}

									break;
								}
						}
					}
					current++;
				}

				sheet.Cells.Style.WrapText = true;
			}
			else
			{
				sheet.Cells[startRow, startColumn].LoadFromCollection(table, true, TableStyles.Light15).AutoFitColumns(12, 90);
				sheet.Cells.Style.WrapText = true;
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
