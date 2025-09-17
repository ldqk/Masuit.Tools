# Masuit.Tools.DigitalWatermarker
## 简介

Masuit.Tools.DigitalWatermarker 是一个基于 OpenCV 的数字水印库，提供鲁棒的图像数字水印嵌入和提取功能。该库采用 DCT（离散余弦变换）和 QIM（量化索引调制）技术，可以在图像中嵌入不可见的数字水印，用于版权保护和内容追溯。

## 特性

- **不可见水印**：水印嵌入在图像的频域中，肉眼不可见
- **鲁棒性强**：对图像压缩、裁剪、翻转、翻录等攻击具有较强的抵抗能力
- **自适应强度**：根据图像块的能量自适应调整嵌入强度
- **简单易用**：提供简洁的 API，支持文件路径、流等多种输入方式
- **高性能**：基于 OpenCV 实现，处理速度快

## 技术原理

该库采用以下关键技术：

1. **DCT变换**：在 8×8 像素块上进行离散余弦变换
2. **中频嵌入**：选择中频系数位置 (2,3) 和 (3,2) 进行水印嵌入
3. **QIM调制**：使用量化索引调制技术，提高抗压缩能力
4. **YCrCb色彩空间**：仅在亮度通道嵌入，最大化视觉不可感知性
5. **自适应阈值**：根据图像块的 DCT 能量自适应调整嵌入强度

## 安装

通过 NuGet 安装：

```bash
Install-Package Masuit.Tools.DigitalWatermarker
Install-Package OpenCvSharp4.runtime.win
```

或者使用 .NET CLI：

```bash
dotnet add package Masuit.Tools.DigitalWatermarker
dotnet add package OpenCvSharp4.runtime.win
```

## 使用方法

### 基本用法

```csharp
using Masuit.Tools.DigtalWatermarker;
using OpenCvSharp;

// 从文件嵌入水印
Mat watermarkedImage = DigitalWatermarker.EmbedWatermark("source.jpg", "watermark.png");
Cv2.ImWrite("watermarked.jpg", watermarkedImage);

// 提取水印
Mat extractedWatermark = DigitalWatermarker.ExtractWatermark("watermarked.jpg");
Cv2.ImWrite("extracted_watermark.png", extractedWatermark);
```

### 使用 Mat 对象

```csharp
using var source = Cv2.ImRead("source.jpg");
using var watermark = Cv2.ImRead("watermark.png");

// 嵌入水印
using var watermarked = DigitalWatermarker.EmbedWatermark(source, watermark);
Cv2.ImWrite("result.jpg", watermarked);

// 提取水印
using var extracted = DigitalWatermarker.ExtractWatermark(watermarked);
Cv2.ImWrite("extracted.png", extracted);
```

### 使用流

```csharp
using var sourceStream = File.OpenRead("source.jpg");
using var watermarkStream = File.OpenRead("watermark.png");

// 嵌入水印
using var watermarked = DigitalWatermarker.EmbedWatermark(sourceStream, watermarkStream);
Cv2.ImWrite("result.jpg", watermarked);

// 从流提取水印
using var imageStream = File.OpenRead("watermarked.jpg");
using var extracted = DigitalWatermarker.ExtractWatermark(imageStream);
Cv2.ImWrite("extracted.png", extracted);
```

## 最佳实践

### 水印图像要求

1. **尺寸建议**：水印图像会被自动缩放到适合的尺寸（基于 8×8 块网格）
2. **内容建议**：使用高对比度的黑白图像作为水印，效果更佳
3. **格式支持**：支持所有 OpenCV 支持的图像格式

### 使用建议

1. **源图像质量**：建议使用高质量的源图像，避免过度压缩
2. **水印提取**：提取的水印可能包含噪声，可以进行后处理优化
3. **鲁棒性测试**：建议对嵌入水印的图像进行各种攻击测试，验证水印的鲁棒性

### 性能优化

1. **图像尺寸**：处理时间与图像尺寸成正比，大图像需要更多处理时间
2. **内存管理**：及时释放 Mat 对象以避免内存泄漏
3. **批量处理**：如需处理大量图像，建议进行并行处理

## 应用场景

### 版权保护

```csharp
// 为摄影作品添加版权水印
var photographer = "© 2024 Photographer Name";
var logo = Cv2.ImRead("logo.png");
var photo = Cv2.ImRead("photo.jpg");

using var protected_photo = DigitalWatermarker.EmbedWatermark(photo, logo);
Cv2.ImWrite("protected_photo.jpg", protected_photo);
```

### 内容溯源

```csharp
// 为媒体内容添加来源标识
var sourceId = GenerateSourceIdentifier(); // 生成唯一标识图像
var content = Cv2.ImRead("content.jpg");

using var traced_content = DigitalWatermarker.EmbedWatermark(content, sourceId);
// 发布traced_content...

// 后续验证来源
using var extracted_id = DigitalWatermarker.ExtractWatermark(traced_content);
bool isAuthentic = VerifySourceIdentifier(extracted_id);
```

## 限制和注意事项

1. **处理区域**：只处理图像中8的倍数的区域，边缘部分可能被忽略
2. **色彩空间**：仅在亮度通道进行水印嵌入
3. **OpenCV依赖**：需要安装 OpenCV 运行时
4. **攻击抵抗**：虽然具有一定的鲁棒性，但不能抵抗所有类型的攻击

## 系统要求

- .NET Standard 2.0 或更高版本
- OpenCvSharp4 (自动安装)

## 许可证

本项目采用 MIT 许可证。详情请参见 [LICENSE](https://github.com/ldqk/Masuit.Tools/blob/master/LICENSE) 文件。

## 贡献

欢迎提交 Issue 和 Pull Request！

## 相关链接

- [项目主页](https://github.com/ldqk/Masuit.Tools)
- [NuGet 包](https://www.nuget.org/packages/Masuit.Tools.DigitalWatermarker)
- [问题反馈](https://github.com/ldqk/Masuit.Tools/issues)
