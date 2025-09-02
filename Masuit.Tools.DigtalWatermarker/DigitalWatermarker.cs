using System;
using System.IO;
using OpenCvSharp;

namespace Masuit.Tools.DigtalWatermarker;

/// <summary>
/// 数字水印
/// </summary>
public static class DigitalWatermarker
{
    // 配置：块大小与DCT系数对位置（中频），以及嵌入强度
    private const int BlockSize = 8;

    // 选择两处中频系数位置（行,列），避免(0,0)直流与高频边角
    private static readonly (int r, int c) C1 = (2, 3);

    private static readonly (int r, int c) C2 = (3, 2);

    // 嵌入强度基准，越大越鲁棒但越易可见；根据图像块能量自适应缩放
    private const float Alpha = 12.0f;

    /// <summary>
    /// 添加数字水印，将水印图片内容隐藏到图像中，实现图像的版权保护和追溯，当图像被修改或攻击时，如裁剪压缩翻转截屏翻录等操作，水印信息不会受到影响。
    /// </summary>
    /// <param name="source">原始图片</param>
    /// <param name="watermark">水印图片</param>
    /// <returns>被水印保护的新图片</returns>
    public static Mat EmbedWatermark(Mat source, Mat watermark)
    {
        if (source.Empty()) throw new ArgumentException("source is empty", nameof(source));
        if (watermark.Empty()) throw new ArgumentException("watermark is empty", nameof(watermark));

        // 转YCrCb，仅在亮度通道嵌入，最大化视觉不可感知性
        Mat srcBgr = source.Clone();
        Mat ycrcb = new();
        Cv2.CvtColor(srcBgr, ycrcb, ColorConversionCodes.BGR2YCrCb);
        Mat[] planes = Cv2.Split(ycrcb);
        Mat y = planes[0];

        // 保障处理区域为8的倍数
        int w = (y.Cols / BlockSize) * BlockSize;
        int h = (y.Rows / BlockSize) * BlockSize;
        var roi = new Rect(0, 0, w, h);
        Mat yRoi = new(y, roi);

        // 预处理水印：灰度->二值，缩放到块网格大小（每块嵌入1比特）
        int gridW = w / BlockSize;
        int gridH = h / BlockSize;
        Mat wmGray = watermark.Channels() > 1 ? watermark.CvtColor(ColorConversionCodes.BGR2GRAY) : watermark.Clone();
        Mat wmResized = new();
        Cv2.Resize(wmGray, wmResized, new Size(gridW, gridH), 0, 0, InterpolationFlags.Area);
        Mat wmBin = new();
        Cv2.Threshold(wmResized, wmBin, 0, 1, ThresholdTypes.Otsu);

        // 使用32位浮点处理DCT
        Mat yF = new();
        yRoi.ConvertTo(yF, MatType.CV_32F);

        // 遍历每个8x8块进行嵌入（QIM差值量化，更抗压缩）
        for (int by = 0; by < gridH; by++)
        {
            for (int bx = 0; bx < gridW; bx++)
            {
                int bit = wmBin.Get<byte>(by, bx) > 0 ? 1 : 0;

                var blockRect = new Rect(bx * BlockSize, by * BlockSize, BlockSize, BlockSize);
                using var block = new Mat(yF, blockRect);
                using Mat dct = block.Clone();
                Cv2.Dct(dct, dct);

                // 自适应阈值尺度（依据块DCT能量）
                using var absBlock = new Mat();
                Cv2.Absdiff(dct, 0, absBlock);
                double meanAbs = Cv2.Mean(absBlock)[0];
                float delta = (float)(Alpha * Math.Max(1.0, meanAbs / 12.0));

                float c1 = dct.Get<float>(C1.r, C1.c);
                float c2 = dct.Get<float>(C2.r, C2.c);
                float mid = (c1 + c2) / 2f;
                float diff = c1 - c2;

                // QIM：将差值量化到间隔为2*delta的格点，并根据bit偏置到 +delta 或 -delta
                float step = 2f * delta;
                float q = (float)Math.Round(diff / step);
                float targetDiff = q * step + (bit == 1 ? +delta : -delta);

                c1 = mid + targetDiff / 2f;
                c2 = mid - targetDiff / 2f;

                dct.Set(C1.r, C1.c, c1);
                dct.Set(C2.r, C2.c, c2);

                // 逆DCT写回
                Cv2.Dct(dct, block, DctFlags.Inverse);
            }
        }

        // 合并回亮度通道
        Mat yU8 = new();
        yF.ConvertTo(yU8, MatType.CV_8U);
        yU8.CopyTo(new Mat(y, roi));

        planes[0] = y; // 亮度已更新
        Cv2.Merge(planes, ycrcb);
        Mat result = new();
        Cv2.CvtColor(ycrcb, result, ColorConversionCodes.YCrCb2BGR);
        return result;
    }

    /// <summary>
    /// 提取水印内容，从被水印保护的图像中提取隐藏的数字水印信息。
    /// </summary>
    /// <param name="image">被保护的图像</param>
    /// <returns>水印内容</returns>
    public static Mat ExtractWatermark(Mat image)
    {
        if (image.Empty()) throw new ArgumentException("image is empty", nameof(image));

        // 转Y通道
        Mat ycrcb = new();
        Cv2.CvtColor(image, ycrcb, ColorConversionCodes.BGR2YCrCb);
        var planes = Cv2.Split(ycrcb);
        Mat y = planes[0];

        int w = (y.Cols / BlockSize) * BlockSize;
        int h = (y.Rows / BlockSize) * BlockSize;
        var roi = new Rect(0, 0, w, h);
        Mat yRoi = new(y, roi);
        int gridW = w / BlockSize;
        int gridH = h / BlockSize;

        Mat yF = new();
        yRoi.ConvertTo(yF, MatType.CV_32F);

        Mat wm = new(gridH, gridW, MatType.CV_8U);

        for (int by = 0; by < gridH; by++)
        {
            for (int bx = 0; bx < gridW; bx++)
            {
                var blockRect = new Rect(bx * BlockSize, by * BlockSize, BlockSize, BlockSize);
                using var block = new Mat(yF, blockRect);
                using Mat dct = block.Clone();
                Cv2.Dct(dct, dct);

                float c1 = dct.Get<float>(C1.r, C1.c);
                float c2 = dct.Get<float>(C2.r, C2.c);
                byte bit = (byte)(c1 > c2 ? 255 : 0);
                wm.Set(by, bx, bit);
            }
        }

        // 轻微中值滤波平滑噪声
        Mat wmOut = new();
        Cv2.MedianBlur(wm, wmOut, 3);
        return wmOut;
    }

    /// <summary>
    /// 从文件路径读取图片并添加数字水印。
    /// </summary>
    /// <param name="sourceImagePath">原始图片路径</param>
    /// <param name="watermarkImagePath">水印图片路径</param>
    /// <returns>被水印保护的新图片</returns>
    public static Mat EmbedWatermark(string sourceImagePath, string watermarkImagePath)
    {
        if (string.IsNullOrWhiteSpace(sourceImagePath)) throw new ArgumentException("图片路径不能为空", nameof(sourceImagePath));
        if (string.IsNullOrWhiteSpace(watermarkImagePath)) throw new ArgumentException("水印图片路径不能为空", nameof(watermarkImagePath));
        if (!File.Exists(sourceImagePath)) throw new FileNotFoundException("文件不存在", sourceImagePath);
        if (!File.Exists(watermarkImagePath)) throw new FileNotFoundException("文件不存在", watermarkImagePath);

        using var src = Cv2.ImRead(sourceImagePath);
        using var wm = Cv2.ImRead(watermarkImagePath);
        return EmbedWatermark(src, wm);
    }

    /// <summary>
    /// 从两个流中读取图片并添加数字水印。
    /// </summary>
    /// <param name="sourceStream">原始图片流</param>
    /// <param name="watermarkStream">水印图片流</param>
    /// <returns>被水印保护的新图片</returns>
    public static Mat EmbedWatermark(Stream sourceStream, Stream watermarkStream)
    {
        if (sourceStream is null) throw new ArgumentNullException(nameof(sourceStream));
        if (watermarkStream is null) throw new ArgumentNullException(nameof(watermarkStream));

        using var src = ReadMatFromStream(sourceStream, ImreadModes.Color);
        using var wm = ReadMatFromStream(watermarkStream, ImreadModes.Color);
        return EmbedWatermark(src, wm);
    }

    /// <summary>
    /// 从文件路径读取图像并提取数字水印。
    /// </summary>
    /// <param name="imagePath">被保护的图像路径</param>
    /// <returns>水印内容</returns>
    public static Mat ExtractWatermark(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath)) throw new ArgumentException("路径不能为空", nameof(imagePath));
        if (!File.Exists(imagePath)) throw new FileNotFoundException("文件不存在", imagePath);

        using var img = Cv2.ImRead(imagePath, ImreadModes.Color);
        return ExtractWatermark(img);
    }

    /// <summary>
    /// 从流中读取图像并提取数字水印。
    /// </summary>
    /// <param name="imageStream">被保护的图像流</param>
    /// <returns>水印内容</returns>
    public static Mat ExtractWatermark(Stream imageStream)
    {
        if (imageStream is null) throw new ArgumentNullException(nameof(imageStream));
        using var img = ReadMatFromStream(imageStream, ImreadModes.Color);
        return ExtractWatermark(img);
    }

    // 辅助：从 Stream 读取为 Mat
    private static Mat ReadMatFromStream(Stream stream, ImreadModes mode)
    {
        if (stream is null) throw new ArgumentNullException(nameof(stream));
        if (!stream.CanRead) throw new ArgumentException("Stream不可用", nameof(stream));

        byte[] bytes;
        if (stream is MemoryStream ms)
        {
            bytes = ms.ToArray();
        }
        else
        {
            using var tmp = new MemoryStream();
            stream.CopyTo(tmp);
            bytes = tmp.ToArray();
        }

        var img = Cv2.ImDecode(bytes, mode);
        if (img.Empty())
            throw new ArgumentException("stream不能解析为图像", nameof(stream));
        return img;
    }
}