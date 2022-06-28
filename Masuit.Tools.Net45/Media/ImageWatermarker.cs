using System;
using System.Drawing;
using System.Drawing.Text;
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

        /// <summary>
        /// 添加水印
        /// </summary>
        /// <param name="watermarkText">水印文字</param>
        /// <param name="color">水印颜色</param>
        /// <param name="watermarkPosition">水印位置</param>
        /// <param name="textPadding">边距</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="font">字体</param>
        /// <param name="textAntiAlias">不提示的情况下使用抗锯齿标志符号位图来绘制每个字符。
        ///    由于抗锯齿质量就越好。
        ///    因为关闭了提示，词干宽度之间的差异可能非常明显。</param>
        /// <returns></returns>
        public MemoryStream AddWatermark(string watermarkText, Color color, WatermarkPosition watermarkPosition = WatermarkPosition.BottomRight, int textPadding = 10, int fontSize = 20, Font font = null, bool textAntiAlias = true)
        {
            using var img = Image.FromStream(_stream);
            if (SkipWatermarkForSmallImages && (img.Height < Math.Sqrt(SmallImagePixelsThreshold) || img.Width < Math.Sqrt(SmallImagePixelsThreshold)))
            {
                return _stream.SaveAsMemoryStream();
            }

            using var graphic = Graphics.FromImage(img);
            if (textAntiAlias)
            {
                graphic.TextRenderingHint = TextRenderingHint.AntiAlias;
            }

            using var brush = new SolidBrush(color);
            if (img.Width / fontSize > 50)
            {
                fontSize = img.Width / 50;
            }

            using var f = font ?? new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
            var textSize = graphic.MeasureString(watermarkText, f);
            int x, y;
            textPadding += (img.Width - 1000) / 100;
            switch (watermarkPosition)
            {
                case WatermarkPosition.TopLeft:
                    x = textPadding;
                    y = textPadding;
                    break;

                case WatermarkPosition.TopRight:
                    x = img.Width - (int)textSize.Width - textPadding;
                    y = textPadding;
                    break;

                case WatermarkPosition.BottomLeft:
                    x = textPadding;
                    y = img.Height - (int)textSize.Height - textPadding;
                    break;

                case WatermarkPosition.BottomRight:
                    x = img.Width - (int)textSize.Width - textPadding;
                    y = img.Height - (int)textSize.Height - textPadding;
                    break;

                default:
                    x = textPadding;
                    y = textPadding;
                    break;
            }

            graphic.DrawString(watermarkText, f, brush, new Point(x, y));
            var ms = new MemoryStream();
            img.Save(ms, img.RawFormat);
            ms.Position = 0;
            return ms;
        }
    }
}
