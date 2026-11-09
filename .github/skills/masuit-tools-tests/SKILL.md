---
name: masuit-tools-tests
description: "在 Masuit.Tools 仓库新增或运行测试、验证跨包改动、处理构建失败或确认回归行为时使用。"
---

# Masuit.Tools 测试与验证

仓库测试按包分布在 `Test` 下。先选择与改动所属项目对应的测试项目，再扩大验证范围。

## 测试项目路由

| 改动 | 定向测试项目 |
|---|---|
| Abstractions 通用工具 | `Test\Masuit.Tools.Abstractions.Test` |
| Core / EF Core / ASP.NET Core | `Test\Masuit.Tools.Core.Test` |
| Excel | `Test\Masuit.Tools.Excel.Test` |
| 数字水印 | `Test\Masuit.Tools.DigitalWatermarker.Test` |
| 旧版 `Masuit.Tools.Net` / MVC / .NET Framework | `Test\Masuit.Tools.Test`（传统非 SDK 项目，目标为 .NET Framework 4.8.1） |
| 断点续传 Web 示例 | `Test\Masuit.Tools.AspNetCore.ResumeFileResults.WebTest` |

现代测试项目以 xUnit 为主，多数目标 `net10.0`；旧版测试项目使用传统 MSBuild 格式。不要假定解决方案中的全部项目可在任意机器/SDK 上一次性测试。

## 推荐流程

1. 为错误行为新增一个失败测试，验证修复后的输出/副作用，而非只测实现细节。
2. 覆盖边界输入、异常及调用方可观察行为；文件/HTTP/数据库功能优先使用仓库已采用的测试宿主或 InMemory 组件。
3. 运行最小相关测试项目。跨包改动则逐个运行所有被影响测试。
4. 对受影响的生产项目运行 `dotnet build <项目.csproj>`；多目标项目检查是否需显式 `-f <TFM>` 才能覆盖关键框架。
5. 解释无法运行的测试及其环境依赖；不要把跳过、未发现测试或环境故障报告为通过。

## 常用命令

```powershell
dotnet test Test\Masuit.Tools.Abstractions.Test\Masuit.Tools.Abstractions.Test.csproj
dotnet test Test\Masuit.Tools.Core.Test\Masuit.Tools.Core.Test.csproj
dotnet test Test\Masuit.Tools.Excel.Test\Masuit.Tools.Excel.Test.csproj
dotnet test Test\Masuit.Tools.DigitalWatermarker.Test\Masuit.Tools.DigitalWatermarker.Test.csproj
dotnet build Masuit.Tools.sln
```

`Masuit.Tools.Test` 使用传统 .NET Framework 项目格式，运行前确认机器已安装对应的 .NET Framework targeting pack 和 Visual Studio 测试工具。数字水印测试需要项目声明的 OpenCvSharp Windows 原生运行时。

`BenchmarkTest` 是性能基准项目，不替代功能回归测试。仅性能相关改动才需要运行基准；对基准结果应记录环境并与相同条件下的基线比较。
