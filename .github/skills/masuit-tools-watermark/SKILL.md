---
name: masuit-tools-watermark
description: "在 Masuit.Tools.DigtalWatermarker 中实现、调试或测试数字图像水印、OpenCvSharp 图像处理和相关包行为时使用。"
---

# Masuit.Tools 数字水印

独立项目目录拼写为 `Masuit.Tools.DigtalWatermarker`（项目和包 ID 也使用 `Digtal`），目标框架为 `netstandard2.0`，依赖 OpenCvSharp4。测试项目目录为 `Test\Masuit.Tools.DigitalWatermarker.Test`，名称拼写为 `DigitalWatermarker`。不要在无关改动中重命名这些现存项目。

## 工作流程

1. 查阅该项目 README、实现和测试，确认水印模式、参数和输入输出约定；不要凭名称假定算法细节。
2. 验证图像输入、尺寸/通道、空值、编码格式、质量和异常路径。
3. 正确处理 OpenCV 原生资源及 Mat/图像缓冲区生命周期，避免泄漏或返回已释放资源。
4. 对算法改动增加固定输入的回归测试；兼顾不同尺寸/格式，并避免仅以“无异常”作为正确性的唯一标准。
5. 运行 Windows 原生运行时测试及项目构建；测试依赖 OpenCvSharp Windows runtime，注意运行环境要求。

## 定向验证

```powershell
dotnet test Test\Masuit.Tools.DigitalWatermarker.Test\Masuit.Tools.DigitalWatermarker.Test.csproj
dotnet build Masuit.Tools.DigtalWatermarker\Masuit.Tools.DigtalWatermarker.csproj
```

修改 OpenCvSharp 依赖或目标框架时，同时核对库项目和测试项目的运行时包版本。
