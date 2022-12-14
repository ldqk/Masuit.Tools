#if NET461
using System.Web;
#else

using Microsoft.AspNetCore.Http;

#endif

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Systems;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace Masuit.Tools.Strings
{
    /// <summary>
    /// 画验证码
    /// </summary>
    public static class ValidateCode
    {
        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="length">指定验证码的长度</param>
        /// <returns>验证码字符串</returns>
        public static string CreateValidateCode(int length)
        {
            string ch = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ1234567890@#$%&?";
            byte[] b = new byte[4];
            using var cpt = RandomNumberGenerator.Create();
            cpt.GetBytes(b);
            var r = new Random(BitConverter.ToInt32(b, 0));
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(ch[r.StrictNext(ch.Length)]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 创建验证码的图片
        /// </summary>
        /// <param name="validateCode">验证码序列</param>
        /// <param name="context">当前的HttpContext上下文对象</param>
        /// <param name="fontSize">字体大小，默认值28px</param>
        /// <exception cref="Exception">The operation failed.</exception>
        public static byte[] CreateValidateGraphic(this HttpContext context, string validateCode, int fontSize = 28)
        {
            var fontFamily = SystemFonts.Families.Where(f => new[] { "Consolas", "DejaVu Sans", "KaiTi", "NSimSun", "SimSun", "SimHei", "Microsoft YaHei UI", "Arial" }.Contains(f.Name)).OrderByRandom().FirstOrDefault();
            if (fontFamily == default)
            {
                fontFamily = SystemFonts.Families.OrderByRandom().FirstOrDefault();
            }

            var font = fontFamily.CreateFont(fontSize);
            var measure = TextMeasurer.Measure(validateCode, new TextOptions(font));
            var width = (int)Math.Ceiling(measure.Width * 1.5);
            var height = (int)Math.Ceiling(measure.Height + 5);
            using var image = new Image<Rgba32>(width, height);

            //生成随机生成器
            Random random = new Random();

            //清空图片背景色
            image.Mutate(g =>
            {
                g.BackgroundColor(Color.White);

                //画图片的干扰线
                for (int i = 0; i < 75; i++)
                {
                    int x1 = random.StrictNext(width);
                    int x2 = random.StrictNext(width);
                    int y1 = random.StrictNext(height);
                    int y2 = random.StrictNext(height);
                    g.DrawLines(new Pen(new Color(new Rgba32((byte)random.StrictNext(255), (byte)random.StrictNext(255), (byte)random.StrictNext(255))), 1), new PointF(x1, y1), new PointF(x2, y2));
                }

                //渐变.
                var brush = new LinearGradientBrush(new PointF(0, 0), new PointF(width, height), GradientRepetitionMode.Repeat, new ColorStop(0.5f, Color.Blue), new ColorStop(0.5f, Color.DarkRed));
                g.DrawText(validateCode, font, brush, new PointF(3, 2));

                //画图片的边框线
                g.DrawLines(new Pen(Color.Silver, 1), new PointF(0, 0), new PointF(width - 1, height - 1));
            });

            //画图片的前景干扰点
            for (int i = 0; i < 350; i++)
            {
                int x = random.StrictNext(image.Width);
                int y = random.StrictNext(image.Height);
                image[x, y] = new Rgba32(random.StrictNext(255), random.StrictNext(255), random.StrictNext(255));
            }

            //保存图片数据
            using var stream = new PooledMemoryStream();
            image.Save(stream, WebpFormat.Instance);

            //输出图片流
            context.Response.Clear();
            context.Response.ContentType = ContentType.Jpeg;
            return stream.ToArray();
        }

        /// <summary>
        /// 字符串宽度
        /// </summary>
        /// <param name="s"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static float StringWidth(this string s, int fontSize = 1)
        {
            var fontFamily = SystemFonts.Families.FirstOrDefault(f => f.Name == "Microsoft YaHei UI");
            if (fontFamily == default)
            {
                fontFamily = SystemFonts.Families.FirstOrDefault();
            }

            return TextMeasurer.Measure(s, new TextOptions(fontFamily.CreateFont(fontSize))).Width;
        }

        /// <summary>
        /// 字符串宽度
        /// </summary>
        /// <param name="s"></param>
        /// <param name="fontName">字体名字，如：Microsoft YaHei UI</param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static float StringWidth(this string s, string fontName, int fontSize = 1)
        {
            var fontFamily = SystemFonts.Families.FirstOrDefault(f => f.Name == fontName);
            if (fontFamily == default)
            {
                throw new ArgumentException($"字体 {fontName} 不存在，请尝试其它字体！");
            }

            return TextMeasurer.Measure(s, new TextOptions(fontFamily.CreateFont(fontSize))).Width;
        }
    }
}
