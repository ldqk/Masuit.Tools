---
name: masuit-tools-excel
description: "在 Masuit.Tools.Excel 中新增、修改或排查 Excel 数据导入导出、列格式、图片、加密或 ClosedXML 相关行为时使用。"
---

# Masuit.Tools Excel

Excel 是独立包，项目位于 `Masuit.Tools.Excel`，主要实现包括 `ExcelExtension.cs`、`ColumnSettings.cs`、`ColumnFormatAttribute.cs`、`ExportIgnoreAttribute.cs` 与 `NumberFormater.cs`。项目目标框架为 `netstandard2.1`，使用 ClosedXML、Open XML 及 Abstractions。

## 工作流程

1. 搜索现有导出入口、列设置/忽略属性和格式化逻辑，遵守既有公开 API 与默认行为。
2. 判断数据来源（集合、DataTable 等）及输出类型，检查空集合、空值、特殊字符、列顺序、格式、图片和资源释放边界。
3. 涉及 Excel 单元格类型/格式时，确认生成文件在 Excel/Open XML 语义中的实际结果；避免只断言内部对象状态。
4. 在 `Test\Masuit.Tools.Excel.Test` 增加回归用例；如需验证文件内容，可使用现有测试依赖读取生成的工作簿。
5. 运行定向测试并更新根目录 `README.md` 中受影响的 Excel 说明。

## 定向验证

```powershell
dotnet test Test\Masuit.Tools.Excel.Test\Masuit.Tools.Excel.Test.csproj
dotnet build Masuit.Tools.Excel\Masuit.Tools.Excel.csproj
```

改变包依赖时，核对 `Masuit.Tools.Excel.csproj` 中的目标框架、ClosedXML/Open XML 依赖以及 Abstractions 项目引用；不要将 Excel 专属依赖移入基础工具项目。
