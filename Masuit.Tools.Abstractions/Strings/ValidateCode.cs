#if NET461
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Drawing.Imaging;
using Masuit.Tools.AspNetCore.Mime;

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
            using var cpt = new RNGCryptoServiceProvider();
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
        /// <param name="fontSize">字体大小，默认值22px</param>
        /// <param name="lineHeight">行高，默认36px</param>
        /// <exception cref="Exception">The operation failed.</exception>
        public static byte[] CreateValidateGraphic(this HttpContext context, string validateCode, int fontSize = 22, int lineHeight = 36)
        {
            using Bitmap image = new Bitmap((int)Math.Ceiling(validateCode.Length * (fontSize + 2.0)), lineHeight + 2);
            using Graphics g = Graphics.FromImage(image);
            //生成随机生成器
            Random random = new Random();
            //清空图片背景色
            g.Clear(Color.White);
            //画图片的干扰线
            for (int i = 0; i < 75; i++)
            {
                int x1 = random.StrictNext(image.Width);
                int x2 = random.StrictNext(image.Width);
                int y1 = random.StrictNext(image.Height);
                int y2 = random.StrictNext(image.Height);
                g.DrawLine(new Pen(Color.FromArgb(random.StrictNext(255), random.StrictNext(255), random.StrictNext(255))), x1, y1, x2, y2);
            }

            Font[] fonts =
            {
                new Font("Arial", fontSize, FontStyle.Bold | FontStyle.Italic),
                new Font("微软雅黑", fontSize, FontStyle.Bold | FontStyle.Italic),
                new Font("黑体", fontSize, FontStyle.Bold | FontStyle.Italic),
                new Font("宋体", fontSize, FontStyle.Bold | FontStyle.Italic),
                new Font("楷体", fontSize, FontStyle.Bold | FontStyle.Italic)
            };
            //渐变.
            using var brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true);
            g.DrawString(validateCode, fonts[random.StrictNext(fonts.Length)], brush, 3, 2);

            //画图片的前景干扰点
            for (int i = 0; i < 350; i++)
            {
                int x = random.StrictNext(image.Width);
                int y = random.StrictNext(image.Height);
                image.SetPixel(x, y, Color.FromArgb(random.StrictNext(255), random.StrictNext(255), random.StrictNext(255)));
            }

            //画图片的边框线
            g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
            //保存图片数据
            using MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Jpeg);
            //输出图片流
            context.Response.Clear();
            context.Response.ContentType = ContentType.Jpeg;
            return stream.ToArray();
        }
    }
}
