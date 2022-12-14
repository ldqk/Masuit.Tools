using Masuit.Tools.Systems;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.IO;

namespace Masuit.Tools.Media
{
    public class ImageWatermarker
    {
        /// <summary>
        /// 是否跳过小缩略图
        /// </summary>
        public bool SkipWatermarkForSmallImages { get; set; }

        /// <summary>
        /// 小图像素大小
        /// </summary>
        public int SmallImagePixelsThreshold { get; set; }

        private readonly Stream _stream;

        public ImageWatermarker(Stream originStream)
        {
            _stream = originStream;
        }

        public PooledMemoryStream AddWatermark(string watermarkText, string ttfFontPath, int fontSize, Color color, WatermarkPosition watermarkPosition = WatermarkPosition.BottomRight, int textPadding = 10)
        {
            var fonts = new FontCollection();
            var fontFamily = fonts.Add(ttfFontPath); //字体的路径（电脑自带字体库，去copy出来）
            var font = new Font(fontFamily, fontSize, FontStyle.Bold);
            return AddWatermark(watermarkText, font, color, watermarkPosition, textPadding);
        }

        /// <summary>
        /// 添加水印
        /// </summary>
        /// <param name="watermarkText">水印文字</param>
        /// <param name="color">水印颜色</param>
        /// <param name="watermarkPosition">水印位置</param>
        /// <param name="textPadding">边距</param>
        /// <param name="font">字体</param>
        /// <returns></returns>
        public PooledMemoryStream AddWatermark(string watermarkText, Font font, Color color, WatermarkPosition watermarkPosition = WatermarkPosition.BottomRight, int textPadding = 10)
        {
            using var img = Image.Load(_stream);
            var textMeasure = TextMeasurer.Measure(watermarkText, new TextOptions(font));
            if (SkipWatermarkForSmallImages && (img.Height < Math.Sqrt(SmallImagePixelsThreshold) || img.Width < Math.Sqrt(SmallImagePixelsThreshold) || img.Width <= textMeasure.Width))
            {
                return _stream as PooledMemoryStream ?? _stream.SaveAsMemoryStream();
            }

            if (img.Width / font.Size > 50)
            {
                font = font.Family.CreateFont(img.Width * 1f / 50);
            }

            float x, y;
            textPadding += (img.Width - 1000) / 100;
            switch (watermarkPosition)
            {
                case WatermarkPosition.TopRight:
                    x = img.Width - textMeasure.Width - textPadding;
                    y = textPadding;
                    break;

                case WatermarkPosition.BottomLeft:
                    x = textPadding;
                    y = img.Height - textMeasure.Height - textPadding;
                    break;

                case WatermarkPosition.BottomRight:
                    x = img.Width - textMeasure.Width - textPadding;
                    y = img.Height - textMeasure.Height - textPadding;
                    break;

                case WatermarkPosition.Center:
                    x = (img.Width - textMeasure.Width) / 2;
                    y = (img.Height - textMeasure.Height) / 2;
                    break;

                default:
                    x = textPadding;
                    y = textPadding;
                    break;
            }

            img.Mutate(c => c.DrawText(watermarkText, font, color, new PointF(x, y)));
            var ms = new PooledMemoryStream();
            img.SaveAsWebp(ms);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// 添加水印
        /// </summary>
        /// <param name="watermarkImage">水印图片</param>
        /// <param name="opacity">水印图片</param>
        /// <param name="watermarkPosition">水印位置</param>
        /// <param name="padding">水印边距</param>
        /// <returns></returns>
        public PooledMemoryStream AddWatermark(Stream watermarkImage, float opacity = 1f, WatermarkPosition watermarkPosition = WatermarkPosition.BottomRight, int padding = 20)
        {
            using var img = Image.Load(_stream);
            var height = img.Height;
            var width = img.Width;
            if (SkipWatermarkForSmallImages && (height < Math.Sqrt(SmallImagePixelsThreshold) || width < Math.Sqrt(SmallImagePixelsThreshold)))
            {
                return _stream as PooledMemoryStream ?? _stream.SaveAsMemoryStream();
            }

            var watermark = Image.Load(watermarkImage);
            watermark.Mutate(c => c.Resize(new ResizeOptions()
            {
                Size = new Size
                {
                    Width = width / 10,
                    Height = height / 10,
                },
                Mode = ResizeMode.Pad,
                Sampler = new BicubicResampler()
            }));
            int x, y;
            padding += (width - 1000) / 100;
            switch (watermarkPosition)
            {
                case WatermarkPosition.TopRight:
                    x = width - watermark.Width - padding;
                    y = padding;
                    break;

                case WatermarkPosition.BottomLeft:
                    x = padding;
                    y = height - watermark.Height - padding;
                    break;

                case WatermarkPosition.BottomRight:
                    x = width - watermark.Width - padding;
                    y = height - watermark.Height - padding;
                    break;

                case WatermarkPosition.Center:
                    x = (img.Width - watermark.Width) / 2;
                    y = (img.Height - watermark.Height) / 2;
                    break;

                default:
                    x = padding;
                    y = padding;
                    break;
            }

            img.Mutate(c =>
            {
                c.DrawImage(watermark, new Point(x, y), opacity);
                watermark.Dispose();
            });
            var ms = new PooledMemoryStream();
            img.SaveAsWebp(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
