using Masuit.Tools.Systems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;

namespace Masuit.Tools.Media
{
    /// <summary>
    /// 图片处理
    /// </summary>
    public static class ImageUtilities
    {
        #region 判断文件类型是否为WEB格式图片

        /// <summary>
        /// 判断文件类型是否为WEB格式图片
        /// (注：JPG,GIF,BMP,PNG)
        /// </summary>
        /// <param name="contentType">HttpPostedFile.ContentType</param>
        /// <returns>是否为WEB格式图片</returns>
        public static bool IsWebImage(string contentType)
        {
            return contentType == "image/pjpeg" || contentType == "image/jpeg" || contentType == "image/gif" || contentType == "image/bmpp" || contentType == "image/png";
        }

        #endregion 判断文件类型是否为WEB格式图片

        #region 裁剪图片

        /// <summary>
        /// 裁剪图片 -- 用GDI+
        /// </summary>
        /// <param name="b">原始Bitmap</param>
        /// <param name="rec">裁剪区域</param>
        /// <returns>剪裁后的Bitmap</returns>
        public static Image CutImage(this Image b, Rectangle rec)
        {
            b.Mutate(c => c.Crop(rec));
            return b;
        }

        #endregion 裁剪图片

        #region 裁剪并缩放

        /// <summary>
        /// 裁剪并缩放
        /// </summary>
        /// <param name="bmpp">原始图片</param>
        /// <param name="rec">裁剪的矩形区域</param>
        /// <param name="newWidth">新的宽度</param>
        /// <param name="newHeight">新的高度</param>
        /// <returns>处理以后的图片</returns>
        public static Image CutAndResize(this Image bmpp, Rectangle rec, int newWidth, int newHeight)
        {
            bmpp.Mutate(c => c.Crop(rec).Resize(newWidth, newHeight));
            return bmpp;
        }

        #endregion 裁剪并缩放

        #region 缩略图

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImage">原图</param>
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式</param>
        public static void MakeThumbnail(this Image originalImage, string thumbnailPath, int width, int height, ResizeMode mode)
        {
            originalImage.Mutate(c => c.Resize(new ResizeOptions()
            {
                Size = new Size(width, height),
                Mode = mode
            }));
            originalImage.Save(thumbnailPath);
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImage">原图</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式</param>
        public static Image MakeThumbnail(this Image originalImage, int width, int height, ResizeMode mode)
        {
            originalImage.Mutate(c => c.Resize(new ResizeOptions()
            {
                Size = new Size(width, height),
                Mode = mode
            }));
            return originalImage;
        }

        #endregion 缩略图

        #region 调整光暗

        /// <summary>
        /// 调整光暗
        /// </summary>
        /// <param name="source">原始图片</param>
        /// <param name="val">增加或减少的光暗值</param>
        public static Image LDPic(this Image source, int val)
        {
            var copy = source.CloneAs<Rgba32>();
            for (var x = 0; x < copy.Width; x++)
            {
                for (var y = 0; y < copy.Height; y++)
                {
                    var pixel = copy[x, y];
                    var resultR = pixel.R + val; //x、y是循环次数，后面三个是记录红绿蓝三个值的
                    var resultG = pixel.G + val; //x、y是循环次数，后面三个是记录红绿蓝三个值的
                    var resultB = pixel.B + val; //x、y是循环次数，后面三个是记录红绿蓝三个值的
                    copy[x, y] = new Rgba32(resultR, resultG, resultB);
                }
            }

            return copy;
        }

        #endregion 调整光暗

        #region 反色处理

        /// <summary>
        /// 反色处理
        /// </summary>
        /// <param name="source">原始图片</param>
        public static Image RePic(this Image source)
        {
            var copy = source.CloneAs<Rgba32>();
            for (var x = 0; x < copy.Width; x++)
            {
                for (var y = 0; y < copy.Height; y++)
                {
                    var pixel = copy[x, y];
                    var resultR = 255 - pixel.R;
                    var resultG = 255 - pixel.G;
                    var resultB = 255 - pixel.B;
                    copy[x, y] = new Rgba32(resultR, resultG, resultB);
                }
            }

            return copy;
        }

        #endregion 反色处理

        #region 浮雕处理

        /// <summary>
        /// 浮雕处理
        /// </summary>
        /// <param name="oldBitmap">原始图片</param>
        public static Image Relief(this Image oldBitmap)
        {
            var copy = oldBitmap.CloneAs<Rgba32>();
            for (int x = 0; x < copy.Width - 1; x++)
            {
                for (int y = 0; y < copy.Height - 1; y++)
                {
                    var color1 = copy[x, y];
                    var color2 = copy[x + 1, y + 1];
                    var r = Math.Abs(color1.R - color2.R + 128);
                    var g = Math.Abs(color1.G - color2.G + 128);
                    var b = Math.Abs(color1.B - color2.B + 128);
                    if (r > 255) r = 255;
                    if (r < 0) r = 0;
                    if (g > 255) g = 255;
                    if (g < 0) g = 0;
                    if (b > 255) b = 255;
                    if (b < 0) b = 0;
                    copy[x, y] = new Rgba32(r, b, b);
                }
            }

            return copy;
        }

        #endregion 浮雕处理

        #region 拉伸图片

        /// <summary>
        /// 拉伸图片
        /// </summary>
        /// <param name="image">原始图片</param>
        /// <param name="newW">新的宽度</param>
        /// <param name="newH">新的高度</param>
        public static Image ResizeImage(this Image image, int newW, int newH)
        {
            image.Mutate(c => c.Resize(new ResizeOptions()
            {
                Size = new Size(newW, newH),
                Sampler = new BicubicResampler(),
                Mode = ResizeMode.Stretch
            }));
            return image;
        }

        #endregion 拉伸图片

        #region 滤色处理

        /// <summary>
        /// 滤色处理
        /// </summary>
        /// <param name="source">原始图片</param>
        public static Image FilPic(this Image source)
        {
            var copy = source.CloneAs<Rgba32>();
            for (var x = 0; x < copy.Width; x++)
            {
                for (var y = 0; y < copy.Height; y++)
                {
                    copy[x, y] = new Rgba32(0, copy[x, y].G, copy[x, y].B);
                }
            }

            return copy;
        }

        #endregion 滤色处理

        #region 左右翻转

        /// <summary>
        /// 左右翻转
        /// </summary>
        /// <param name="source">原始图片</param>
        public static Image RevPicLR(this Image source)
        {
            source.Mutate(c => c.Flip(FlipMode.Horizontal));
            return source;
        }

        #endregion 左右翻转

        #region 上下翻转

        /// <summary>
        /// 上下翻转
        /// </summary>
        /// <param name="source">原始图片</param>
        public static Image RevPicUD(this Image source)
        {
            source.Mutate(c => c.Flip(FlipMode.Vertical));
            return source;
        }

        #endregion 上下翻转

        #region 灰度化

        /// <summary>
        /// 色彩灰度化
        /// </summary>
        /// <param name="c">输入颜色</param>
        /// <returns>输出颜色</returns>
        public static Color Gray(this Color c)
        {
            var pixel = c.ToPixel<Rgba32>();
            byte rgb = Convert.ToByte(0.3 * pixel.R + 0.59 * pixel.G + 0.11 * pixel.B);
            return Color.FromRgb(rgb, rgb, rgb);
        }

        /// <summary>
        /// 色彩灰度化
        /// </summary>
        /// <param name="c">输入颜色</param>
        /// <returns>输出颜色</returns>
        public static Color Reverse(this Color c)
        {
            var pixel = c.ToPixel<Rgba32>();
            byte w = 255;
            return Color.FromRgba((byte)(w - pixel.R), (byte)(w - pixel.G), (byte)(w - pixel.B), pixel.A);
        }

        #endregion 灰度化

        #region 转换为黑白图片

        /// <summary>
        /// 转换为黑白图片
        /// </summary>
        /// <param name="source">要进行处理的图片</param>
        /// <param name="width">图片的长度</param>
        /// <param name="height">图片的高度</param>
        public static Image BWPic(this Image source, int width, int height)
        {
            source.Mutate(c => c.Resize(new ResizeOptions()
            {
                Size = new Size
                {
                    Width = width,
                    Height = height
                },
                Mode = ResizeMode.Pad,
                Sampler = new BicubicResampler()
            }).BlackWhite());
            return source;
        }

        #endregion 转换为黑白图片

        #region 获取图片中的各帧

        /// <summary>
        /// 获取gif图片中的各帧
        /// </summary>
        /// <param name="gif">源gif</param>
        /// <param name="pSavedPath">保存路径</param>
        public static void GetFrames(this Image gif, string pSavedPath)
        {
            for (var i = 0; i < gif.Frames.Count; i++)
            {
                gif.Frames.ExportFrame(i).Save(pSavedPath + "\\frame_" + i + ".jpg");
            }
        }

        #endregion 获取图片中的各帧

        /// <summary>
        /// 将dataUri保存为图片
        /// </summary>
        /// <param name="source">dataUri数据源</param>
        /// <returns></returns>
        /// <exception cref="Exception">操作失败。</exception>
        public static Image SaveDataUriAsImageFile(this string source)
        {
            string strbase64 = source.Substring(source.IndexOf(',') + 1).Trim('\0');
            byte[] arr = Convert.FromBase64String(strbase64);
            var ms = new PooledMemoryStream(arr);
            return Image.Load(ms);
        }
    }
}
