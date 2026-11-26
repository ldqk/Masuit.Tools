using Masuit.Tools.Systems;
using SkiaSharp;
using System;
using System.IO;

namespace Masuit.Tools.Media
{
    /// <summary>
    /// 图片处理
    /// </summary>
    public static class ImageUtilities
    {
        #region 判断文件类型是否为WEB格式图片

        public static bool IsWebImage(string contentType)
        {
            return contentType == "image/pjpeg" || contentType == "image/jpeg" || contentType == "image/gif" || contentType == "image/bmpp" || contentType == "image/png";
        }

        #endregion

        #region 裁剪图片

        public static SKBitmap CutImage(this SKBitmap b, SKRectI rec)
        {
            var result = new SKBitmap(rec.Width, rec.Height);
            using var canvas = new SKCanvas(result);
            canvas.DrawBitmap(b, rec, new SKRect(0, 0, rec.Width, rec.Height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            return result;
        }

        #endregion

        #region 裁剪并缩放

        public static SKBitmap CutAndResize(this SKBitmap bmpp, SKRectI rec, int newWidth, int newHeight)
        {
            using var cropped = bmpp.CutImage(rec);
            return cropped.Resize(new SKImageInfo(newWidth, newHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        }

        #endregion

        #region 缩略图

        public static void MakeThumbnail(this SKBitmap originalImage, string thumbnailPath, int width, int height, ResizeMode mode)
        {
            using var thumbnail = originalImage.MakeThumbnail(width, height, mode);
            using var data = thumbnail.Encode(SKEncodedImageFormat.Jpeg, 90);
            using var fs = File.OpenWrite(thumbnailPath);
            data.SaveTo(fs);
        }

        public static SKBitmap MakeThumbnail(this SKBitmap originalImage, int width, int height, ResizeMode mode)
        {
            int srcW = originalImage.Width;
            int srcH = originalImage.Height;
            int dstW = width;
            int dstH = height;

            switch (mode)
            {
                case ResizeMode.Stretch:
                    break;
                case ResizeMode.Crop:
                {
                    float ratioW = (float)width / srcW;
                    float ratioH = (float)height / srcH;
                    float ratio = Math.Max(ratioW, ratioH);
                    int scaledW = (int)(srcW * ratio);
                    int scaledH = (int)(srcH * ratio);
                    using var scaled = originalImage.Resize(new SKImageInfo(scaledW, scaledH), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
                    int cropX = (scaledW - width) / 2;
                    int cropY = (scaledH - height) / 2;
                    return scaled.CutImage(new SKRectI(cropX, cropY, cropX + width, cropY + height));
                }
                case ResizeMode.Min:
                {
                    float ratio = Math.Min((float)width / srcW, (float)height / srcH);
                    dstW = (int)(srcW * ratio);
                    dstH = (int)(srcH * ratio);
                    break;
                }
                case ResizeMode.Max:
                {
                    float ratio = Math.Max((float)width / srcW, (float)height / srcH);
                    dstW = (int)(srcW * ratio);
                    dstH = (int)(srcH * ratio);
                    break;
                }
                case ResizeMode.Pad:
                {
                    float ratio = Math.Min((float)width / srcW, (float)height / srcH);
                    int scaledW = (int)(srcW * ratio);
                    int scaledH = (int)(srcH * ratio);
                    var padded = new SKBitmap(width, height);
                    using var canvas = new SKCanvas(padded);
                    canvas.Clear(SKColors.White);
                    using var scaled = originalImage.Resize(new SKImageInfo(scaledW, scaledH), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
                    canvas.DrawBitmap(scaled, (width - scaledW) / 2f, (height - scaledH) / 2f, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
                    return padded;
                }
                case ResizeMode.BoxPad:
                {
                    float ratio = Math.Min((float)width / srcW, (float)height / srcH);
                    int scaledW = (int)(srcW * ratio);
                    int scaledH = (int)(srcH * ratio);
                    var padded = new SKBitmap(width, height);
                    using var canvas = new SKCanvas(padded);
                    canvas.Clear(SKColors.Transparent);
                    using var scaled = originalImage.Resize(new SKImageInfo(scaledW, scaledH), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
                    canvas.DrawBitmap(scaled, (width - scaledW) / 2f, (height - scaledH) / 2f,new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
                    return padded;
                }
            }

            return originalImage.Resize(new SKImageInfo(dstW, dstH), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        }

        #endregion

        #region 调整光暗

        public static SKBitmap LDPic(this SKBitmap source, int val)
        {
            var copy = source.Copy();
            for (var x = 0; x < copy.Width; x++)
            {
                for (var y = 0; y < copy.Height; y++)
                {
                    var pixel = copy.GetPixel(x, y);
                    var r = Clamp(pixel.Red + val);
                    var g = Clamp(pixel.Green + val);
                    var b = Clamp(pixel.Blue + val);
                    copy.SetPixel(x, y, new SKColor(r, g, b, pixel.Alpha));
                }
            }
            return copy;
        }

        #endregion

        #region 反色处理

        public static SKBitmap RePic(this SKBitmap source)
        {
            var copy = source.Copy();
            for (var x = 0; x < copy.Width; x++)
            {
                for (var y = 0; y < copy.Height; y++)
                {
                    var pixel = copy.GetPixel(x, y);
                    copy.SetPixel(x, y, new SKColor((byte)(255 - pixel.Red), (byte)(255 - pixel.Green), (byte)(255 - pixel.Blue), pixel.Alpha));
                }
            }
            return copy;
        }

        #endregion

        #region 浮雕处理

        public static SKBitmap Relief(this SKBitmap oldBitmap)
        {
            var copy = oldBitmap.Copy();
            for (int x = 0; x < copy.Width - 1; x++)
            {
                for (int y = 0; y < copy.Height - 1; y++)
                {
                    var color1 = copy.GetPixel(x, y);
                    var color2 = copy.GetPixel(x + 1, y + 1);
                    var r = Clamp(Math.Abs(color1.Red - color2.Red + 128));
                    var g = Clamp(Math.Abs(color1.Green - color2.Green + 128));
                    var b = Clamp(Math.Abs(color1.Blue - color2.Blue + 128));
                    copy.SetPixel(x, y, new SKColor(r, g, b));
                }
            }
            return copy;
        }

        #endregion

        #region 拉伸图片

        public static SKBitmap ResizeImage(this SKBitmap image, int newW, int newH)
        {
            return image.Resize(new SKImageInfo(newW, newH), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        }

        #endregion

        #region 滤色处理

        public static SKBitmap FilPic(this SKBitmap source)
        {
            var copy = source.Copy();
            for (var x = 0; x < copy.Width; x++)
            {
                for (var y = 0; y < copy.Height; y++)
                {
                    var pixel = copy.GetPixel(x, y);
                    copy.SetPixel(x, y, new SKColor(0, pixel.Green, pixel.Blue));
                }
            }
            return copy;
        }

        #endregion

        /// <summary>
        /// 旋转图片
        /// </summary>
        /// <param name="source"></param>
        /// <param name="degrees">角度</param>
        /// <returns></returns>
        public static SKBitmap Rotate(this SKBitmap source, float degrees)
        {
            var rightAngle = ((int)degrees % 360 + 360) % 360;
            int dstWidth;
            int dstHeight;
            if (rightAngle == 90 || rightAngle == 270)
            {
                dstWidth = source.Height;
                dstHeight = source.Width;
            }
            else
            {
                dstWidth = source.Width;
                dstHeight = source.Height;
            }

            var dest = new SKBitmap(dstWidth, dstHeight, source.ColorType, source.AlphaType);
            using var canvas = new SKCanvas(dest);
            canvas.Clear(SKColors.Transparent);
            canvas.Translate(dstWidth / 2f, dstHeight / 2f);
            canvas.RotateDegrees(degrees);
            canvas.Translate(-source.Width / 2f, -source.Height / 2f);
            canvas.DrawBitmap(source, 0, 0, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            return dest;
        }

        /// <summary>
        /// 水平翻转图片
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static SKBitmap FlipHorizontal(this SKBitmap source)
        {
            var dest = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);
            using var canvas = new SKCanvas(dest);
            canvas.Scale(-1, 1);
            canvas.Translate(-source.Width, 0);
            canvas.DrawBitmap(source, 0, 0);
            return dest;
        }

        /// <summary>
        /// 垂直翻转图片
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static SKBitmap FlipVertical(this SKBitmap source)
        {
            var dest = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);
            using var canvas = new SKCanvas(dest);
            canvas.Scale(1, -1);
            canvas.Translate(0, -source.Height);
            canvas.DrawBitmap(source, 0, 0, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            return dest;
        }

        #region 灰度化

        public static SKColor Gray(this SKColor c)
        {
            byte rgb = Convert.ToByte(0.3 * c.Red + 0.59 * c.Green + 0.11 * c.Blue);
            return new SKColor(rgb, rgb, rgb, c.Alpha);
        }

        public static SKColor Reverse(this SKColor c)
        {
            return new SKColor((byte)(255 - c.Red), (byte)(255 - c.Green), (byte)(255 - c.Blue), c.Alpha);
        }

        #endregion

        #region 转换为黑白图片

        public static SKBitmap BWPic(this SKBitmap source, int width, int height)
        {
            var imageInfo = new SKImageInfo(width, height, SKColorType.Gray8, SKAlphaType.Opaque);
            var resized = new SKBitmap(imageInfo);
            source.ScalePixels(resized, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            // 二值化
            for (int x = 0; x < resized.Width; x++)
            {
                for (int y = 0; y < resized.Height; y++)
                {
                    var pixel = resized.GetPixel(x, y);
                    resized.SetPixel(x, y, pixel.Red > 128 ? SKColors.White : SKColors.Black);
                }
            }
            return resized;
        }

        #endregion

        #region 获取图片中的各帧

        public static void GetFrames(this SKBitmap gif, string pSavedPath)
        {
            // SKBitmap is for a single frame; for GIF frames use SKCodec
            gif.Save(pSavedPath + "\\frame_0.jpg", SKEncodedImageFormat.Jpeg, 90);
        }

        public static void GetFrames(this SKCodec codec, string pSavedPath)
        {
            for (var i = 0; i < codec.FrameCount; i++)
            {
                var info = codec.Info.WithColorType(SKColorType.Rgba8888);
                var frameBitmap = new SKBitmap(info);
                codec.GetPixels(info, frameBitmap.GetPixels(), new SKCodecOptions(i));
                frameBitmap.Save(pSavedPath + "\\frame_" + i + ".jpg", SKEncodedImageFormat.Jpeg, 90);
            }
        }

        #endregion

        public static SKBitmap SaveDataUriAsImageFile(this string source)
        {
            string strbase64 = source.Substring(source.IndexOf(',') + 1).Trim('\0');
            byte[] arr = Convert.FromBase64String(strbase64);
            var ms = new PooledMemoryStream(arr);
            return SKBitmap.Decode(ms);
        }

        public static void Save(this SKBitmap bitmap, string path, SKEncodedImageFormat format, int quality)
        {
            using var data = bitmap.Encode(format, quality);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using var fs = File.OpenWrite(path);
            data.SaveTo(fs);
        }

        private static byte Clamp(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }
    }

    /// <summary>
    /// 缩放模式
    /// </summary>
    public enum ResizeMode
    {
        Stretch,
        Crop,
        Min,
        Max,
        Pad,
        BoxPad,
    }
}
