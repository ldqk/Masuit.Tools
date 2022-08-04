using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Masuit.Tools.Media
{
    /// <summary>
    /// 图片处理
    /// </summary>
    public static class ImageUtilities
    {
        #region 正方型裁剪并缩放

        /// <summary>
        /// 正方型裁剪
        /// 以图片中心为轴心，截取正方型，然后等比缩放
        /// 用于头像处理
        /// </summary>
        /// <param name="fromFile">原图Stream对象</param>
        /// <param name="fileSaveUrl">缩略图存放地址</param>
        /// <param name="side">指定的边长（正方型）</param>
        /// <param name="quality">质量（范围0-100）</param>
        public static void CutForSquare(this Stream fromFile, string fileSaveUrl, int side, int quality)
        {
            //创建目录
            string dir = Path.GetDirectoryName(fileSaveUrl);
            Directory.CreateDirectory(dir);

            //原始图片（获取原始图片创建对象，并使用流中嵌入的颜色管理信息）
            var initImage = Image.FromStream(fromFile, true);

            //原图宽高均小于模版，不作处理，直接保存
            if ((initImage.Width <= side) && (initImage.Height <= side))
            {
                initImage.Save(fileSaveUrl);
            }
            else
            {
                //原始图片的宽、高
                int initWidth = initImage.Width;
                int initHeight = initImage.Height;

                //非正方型先裁剪为正方型
                if (initWidth != initHeight)
                {
                    //截图对象
                    Image pickedImage;
                    Graphics pickedG;

                    //宽大于高的横图
                    if (initWidth > initHeight)
                    {
                        //对象实例化
                        pickedImage = new Bitmap(initHeight, initHeight);
                        pickedG = Graphics.FromImage(pickedImage);

                        //设置质量
                        pickedG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        pickedG.SmoothingMode = SmoothingMode.HighQuality;

                        //定位
                        Rectangle fromR = new Rectangle((initWidth - initHeight) / 2, 0, initHeight, initHeight);
                        Rectangle toR = new Rectangle(0, 0, initHeight, initHeight);

                        //画图
                        pickedG.DrawImage(initImage, toR, fromR, GraphicsUnit.Pixel);

                        //重置宽
                        initWidth = initHeight;
                    }

                    //高大于宽的竖图
                    else
                    {
                        //对象实例化
                        pickedImage = new Bitmap(initWidth, initWidth);
                        pickedG = Graphics.FromImage(pickedImage);

                        //设置质量
                        pickedG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        pickedG.SmoothingMode = SmoothingMode.HighQuality;

                        //定位
                        Rectangle fromR = new Rectangle(0, (initHeight - initWidth) / 2, initWidth, initWidth);
                        Rectangle toR = new Rectangle(0, 0, initWidth, initWidth);

                        //画图
                        pickedG.DrawImage(initImage, toR, fromR, GraphicsUnit.Pixel);

                        //重置高
                        initHeight = initWidth;
                    }

                    //将截图对象赋给原图
                    initImage = (Image)pickedImage.Clone();

                    //释放截图资源
                    initImage.Dispose();
                    pickedG.Dispose();
                    pickedImage.Dispose();
                }

                //缩略图对象
                using Image resultImage = new Bitmap(side, side);
                using var resultG = Graphics.FromImage(resultImage);

                //设置质量
                resultG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                resultG.SmoothingMode = SmoothingMode.HighQuality;

                //用指定背景色清空画布
                resultG.Clear(Color.White);

                //绘制缩略图
                resultG.DrawImage(initImage, new Rectangle(0, 0, side, side), new Rectangle(0, 0, initWidth, initHeight), GraphicsUnit.Pixel);

                //关键质量控制
                //获取系统编码类型数组,包含了jpeg,bmpp,png,gif,tiff
                var icis = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo ici = null;
                foreach (var i in icis)
                {
                    if ((i.MimeType == "image/jpeg") || (i.MimeType == "image/bmpp") || (i.MimeType == "image/png") || (i.MimeType == "image/gif"))
                        ici = i;
                }

                using var ep = new EncoderParameters(1)
                {
                    Param =
                    {
                        [0] = new EncoderParameter(Encoder.Quality, quality)
                    }
                };

                //保存缩略图
                resultImage.Save(fileSaveUrl, ici, ep);
            }
        }

        #endregion 正方型裁剪并缩放

        #region 自定义裁剪并缩放

        /// <summary>
        /// 指定长宽裁剪
        /// 按模版比例最大范围的裁剪图片并缩放至模版尺寸
        /// </summary>
        /// <param name="fromFile">原图Stream对象</param>
        /// <param name="fileSaveUrl">保存路径</param>
        /// <param name="maxWidth">最大宽(单位:px)</param>
        /// <param name="maxHeight">最大高(单位:px)</param>
        /// <param name="quality">质量（范围0-100）</param>
        public static void CutForCustom(this Stream fromFile, string fileSaveUrl, int maxWidth, int maxHeight, int quality)
        {
            //从文件获取原始图片，并使用流中嵌入的颜色管理信息
            using var initImage = Image.FromStream(fromFile, true);

            //原图宽高均小于模版，不作处理，直接保存
            if ((initImage.Width <= maxWidth) && (initImage.Height <= maxHeight))
            {
                initImage.Save(fileSaveUrl);
            }
            else
            {
                //模版的宽高比例
                double templateRate = (double)maxWidth / maxHeight;

                //原图片的宽高比例
                double initRate = (double)initImage.Width / initImage.Height;

                //原图与模版比例相等，直接缩放
                if (templateRate == initRate)
                {
                    //按模版大小生成最终图片
                    Image templateImage = new Bitmap(maxWidth, maxHeight);
                    Graphics templateG = Graphics.FromImage(templateImage);
                    templateG.InterpolationMode = InterpolationMode.High;
                    templateG.SmoothingMode = SmoothingMode.HighQuality;
                    templateG.Clear(Color.White);
                    templateG.DrawImage(initImage, new Rectangle(0, 0, maxWidth, maxHeight), new Rectangle(0, 0, initImage.Width, initImage.Height), GraphicsUnit.Pixel);
                    templateImage.Save(fileSaveUrl, initImage.RawFormat);
                }

                //原图与模版比例不等，裁剪后缩放
                else
                {
                    //裁剪对象
                    Image pickedImage;
                    Graphics pickedG;

                    //定位
                    Rectangle fromR = new Rectangle(0, 0, 0, 0); //原图裁剪定位
                    Rectangle toR = new Rectangle(0, 0, 0, 0); //目标定位

                    //宽为标准进行裁剪
                    if (templateRate > initRate)
                    {
                        //裁剪对象实例化
                        pickedImage = new Bitmap(initImage.Width, (int)Math.Floor(initImage.Width / templateRate));
                        pickedG = Graphics.FromImage(pickedImage);

                        //裁剪源定位
                        fromR.X = 0;
                        fromR.Y = (int)Math.Floor((initImage.Height - initImage.Width / templateRate) / 2);
                        fromR.Width = initImage.Width;
                        fromR.Height = (int)Math.Floor(initImage.Width / templateRate);

                        //裁剪目标定位
                        toR.X = 0;
                        toR.Y = 0;
                        toR.Width = initImage.Width;
                        toR.Height = (int)Math.Floor(initImage.Width / templateRate);
                    }

                    //高为标准进行裁剪
                    else
                    {
                        pickedImage = new Bitmap((int)Math.Floor(initImage.Height * templateRate), initImage.Height);
                        pickedG = Graphics.FromImage(pickedImage);

                        fromR.X = (int)Math.Floor((initImage.Width - initImage.Height * templateRate) / 2);
                        fromR.Y = 0;
                        fromR.Width = (int)Math.Floor(initImage.Height * templateRate);
                        fromR.Height = initImage.Height;

                        toR.X = 0;
                        toR.Y = 0;
                        toR.Width = (int)Math.Floor(initImage.Height * templateRate);
                        toR.Height = initImage.Height;
                    }

                    //设置质量
                    pickedG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    pickedG.SmoothingMode = SmoothingMode.HighQuality;

                    //裁剪
                    pickedG.DrawImage(initImage, toR, fromR, GraphicsUnit.Pixel);

                    //按模版大小生成最终图片
                    using Image templateImage = new Bitmap(maxWidth, maxHeight);
                    using Graphics templateG = Graphics.FromImage(templateImage);
                    templateG.InterpolationMode = InterpolationMode.High;
                    templateG.SmoothingMode = SmoothingMode.HighQuality;
                    templateG.Clear(Color.White);
                    templateG.DrawImage(pickedImage, new Rectangle(0, 0, maxWidth, maxHeight), new Rectangle(0, 0, pickedImage.Width, pickedImage.Height), GraphicsUnit.Pixel);

                    //关键质量控制
                    //获取系统编码类型数组,包含了jpeg,bmpp,png,gif,tiff
                    ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo ici = null;
                    foreach (var i in icis)
                    {
                        if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmpp" || i.MimeType == "image/png" || i.MimeType == "image/gif")
                            ici = i;
                    }

                    EncoderParameters ep = new EncoderParameters(1);
                    ep.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                    //保存缩略图
                    templateImage.Save(fileSaveUrl, ici, ep);
                    pickedG.Dispose();
                    pickedImage.Dispose();
                }
            }
        }

        #endregion 自定义裁剪并缩放

        #region 等比缩放

        /// <summary>
        /// 图片等比缩放
        /// </summary>
        /// <param name="fromFile">原图Stream对象</param>
        /// <param name="savePath">缩略图存放地址</param>
        /// <param name="targetWidth">指定的最大宽度</param>
        /// <param name="targetHeight">指定的最大高度</param>
        /// <param name="watermarkText">水印文字(为""表示不使用水印)</param>
        /// <param name="watermarkImage">水印图片路径(为""表示不使用水印)</param>
        public static void ZoomAuto(this Stream fromFile, string savePath, double targetWidth, double targetHeight, string watermarkText, string watermarkImage)
        {
            //创建目录
            string dir = Path.GetDirectoryName(savePath);
            Directory.CreateDirectory(dir);

            //原始图片（获取原始图片创建对象，并使用流中嵌入的颜色管理信息）
            using Image initImage = Image.FromStream(fromFile, true);

            //原图宽高均小于模版，不作处理，直接保存
            if ((initImage.Width <= targetWidth) && (initImage.Height <= targetHeight))
            {
                //文字水印
                if (!string.IsNullOrEmpty(watermarkText))
                {
                    using var gWater = Graphics.FromImage(initImage);
                    Font fontWater = new Font("黑体", 10);
                    Brush brushWater = new SolidBrush(Color.White);
                    gWater.DrawString(watermarkText, fontWater, brushWater, 10, 10);
                    gWater.Dispose();
                }

                //透明图片水印
                if (!string.IsNullOrEmpty(watermarkImage))
                {
                    if (File.Exists(watermarkImage))
                    {
                        using var wrImage = Image.FromFile(watermarkImage);

                        //水印绘制条件：原始图片宽高均大于或等于水印图片
                        if ((initImage.Width >= wrImage.Width) && (initImage.Height >= wrImage.Height))
                        {
                            Graphics gWater = Graphics.FromImage(initImage);

                            //透明属性
                            ImageAttributes imgAttributes = new ImageAttributes();
                            ColorMap colorMap = new ColorMap();
                            colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                            colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                            ColorMap[] remapTable = { colorMap };
                            imgAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                            float[][] colorMatrixElements =
                            {
                                new[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                                new[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},
                                new[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},
                                new[] {0.0f, 0.0f, 0.0f, 0.5f, 0.0f}, //透明度:0.5
                                new[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}
                            };

                            ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);
                            imgAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                            gWater.DrawImage(wrImage, new Rectangle(initImage.Width - wrImage.Width, initImage.Height - wrImage.Height, wrImage.Width, wrImage.Height), 0, 0, wrImage.Width, wrImage.Height, GraphicsUnit.Pixel, imgAttributes);

                            gWater.Dispose();
                        }
                        wrImage.Dispose();
                    }
                }

                //保存
                initImage.Save(savePath, initImage.RawFormat);
            }
            else
            {
                //缩略图宽、高计算
                double newWidth = initImage.Width;
                double newHeight = initImage.Height;

                //宽大于高或宽等于高（横图或正方）
                if ((initImage.Width > initImage.Height) || (initImage.Width == initImage.Height))
                {
                    //如果宽大于模版
                    if (initImage.Width > targetWidth)
                    {
                        //宽按模版，高按比例缩放
                        newWidth = targetWidth;
                        newHeight = initImage.Height * (targetWidth / initImage.Width);
                    }
                }

                //高大于宽（竖图）
                else
                {
                    //如果高大于模版
                    if (initImage.Height > targetHeight)
                    {
                        //高按模版，宽按比例缩放
                        newHeight = targetHeight;
                        newWidth = initImage.Width * (targetHeight / initImage.Height);
                    }
                }

                //生成新图
                //新建一个bmpp图片
                using Image newImage = new Bitmap((int)newWidth, (int)newHeight);

                //新建一个画板
                using Graphics newG = Graphics.FromImage(newImage);

                //设置质量
                newG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                newG.SmoothingMode = SmoothingMode.HighQuality;

                //置背景色
                newG.Clear(Color.White);

                //画图
                newG.DrawImage(initImage, new Rectangle(0, 0, newImage.Width, newImage.Height), new Rectangle(0, 0, initImage.Width, initImage.Height), GraphicsUnit.Pixel);

                //文字水印
                if (!string.IsNullOrEmpty(watermarkText))
                {
                    using var gWater = Graphics.FromImage(newImage);
                    Font fontWater = new Font("微软雅黑", 10);
                    Brush brushWater = new SolidBrush(Color.White);
                    gWater.DrawString(watermarkText, fontWater, brushWater, 10, 10);
                    gWater.Dispose();
                }

                //透明图片水印
                if (!string.IsNullOrEmpty(watermarkImage))
                {
                    if (File.Exists(watermarkImage))
                    {
                        using Image wrImage = Image.FromFile(watermarkImage);

                        //水印绘制条件：原始图片宽高均大于或等于水印图片
                        if ((newImage.Width >= wrImage.Width) && (newImage.Height >= wrImage.Height))
                        {
                            Graphics gWater = Graphics.FromImage(newImage);

                            //透明属性
                            ImageAttributes imgAttributes = new ImageAttributes();
                            ColorMap colorMap = new ColorMap
                            {
                                OldColor = Color.FromArgb(255, 0, 255, 0),
                                NewColor = Color.FromArgb(0, 0, 0, 0)
                            };
                            ColorMap[] remapTable = { colorMap };
                            imgAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                            float[][] colorMatrixElements =
                            {
                                new[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                                new[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},
                                new[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},
                                new[] {0.0f, 0.0f, 0.0f, 0.5f, 0.0f}, //透明度:0.5
                                new[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}
                            };

                            ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);
                            imgAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                            gWater.DrawImage(wrImage, new Rectangle(newImage.Width - wrImage.Width, newImage.Height - wrImage.Height, wrImage.Width, wrImage.Height), 0, 0, wrImage.Width, wrImage.Height, GraphicsUnit.Pixel, imgAttributes);
                            gWater.Dispose();
                        }
                    }
                }

                //保存缩略图
                newImage.Save(savePath, initImage.RawFormat);
            }
        }

        #endregion 等比缩放

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
        public static Bitmap CutImage(this Bitmap b, Rectangle rec)
        {
            int w = b.Width;
            int h = b.Height;
            if (rec.X >= w || rec.Y >= h)
            {
                return null;
            }

            if (rec.X + rec.Width > w)
            {
                rec.Width = w - rec.X;
            }

            if (rec.Y + rec.Height > h)
            {
                rec.Height = h - rec.Y;
            }

            try
            {
                var bmppOut = new Bitmap(rec.Width, rec.Height, PixelFormat.Format24bppRgb);
                using var g = Graphics.FromImage(bmppOut);
                g.DrawImage(b, new Rectangle(0, 0, rec.Width, rec.Height), new Rectangle(rec.X, rec.Y, rec.Width, rec.Height), GraphicsUnit.Pixel);
                return bmppOut;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion 裁剪图片

        #region 缩放图片

        /// <summary>
        ///  Resize图片
        /// </summary>
        /// <param name="bmpp">原始Bitmap </param>
        /// <param name="newWidth">新的宽度</param>
        /// <param name="newHeight">新的高度</param>
        /// <returns>处理以后的图片</returns>
        public static Bitmap ResizeImage(this Bitmap bmpp, int newWidth, int newHeight)
        {
            try
            {
                var b = new Bitmap(newWidth, newHeight);
                using var g = Graphics.FromImage(b);

                // 插值算法的质量
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmpp, new Rectangle(0, 0, newWidth, newHeight), new Rectangle(0, 0, bmpp.Width, bmpp.Height), GraphicsUnit.Pixel);
                return b;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion 缩放图片

        #region 裁剪并缩放

        /// <summary>
        /// 裁剪并缩放
        /// </summary>
        /// <param name="bmpp">原始图片</param>
        /// <param name="rec">裁剪的矩形区域</param>
        /// <param name="newWidth">新的宽度</param>
        /// <param name="newHeight">新的高度</param>
        /// <returns>处理以后的图片</returns>
        public static Bitmap CutAndResize(this Bitmap bmpp, Rectangle rec, int newWidth, int newHeight) => bmpp.CutImage(rec).ResizeImage(newWidth, newHeight);

        #endregion 裁剪并缩放

        #region 无损压缩图片

        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="sFile">原图片地址</param>
        /// <param name="dFile">压缩后保存图片地址</param>
        /// <param name="quality">压缩质量（数字越小压缩率越高）1-100</param>
        /// <param name="size">压缩后图片的最大大小</param>
        /// <param name="sfsc">是否是第一次调用</param>
        /// <returns></returns>
        public static bool CompressImage(string sFile, string dFile, byte quality = 90, int size = 1024, bool sfsc = true)
        {
            //如果是第一次调用，原始图像的大小小于要压缩的大小，则直接复制文件，并且返回true
            var firstFileInfo = new FileInfo(sFile);
            if (sfsc && firstFileInfo.Length < size * 1024)
            {
                firstFileInfo.CopyTo(dFile);
                return true;
            }

            using Image iSource = Image.FromFile(sFile);
            int dHeight = iSource.Height;
            int dWidth = iSource.Width;
            int sW, sH;

            //按比例缩放
            Size temSize = new Size(iSource.Width, iSource.Height);
            if (temSize.Width > dHeight || temSize.Width > dWidth)
            {
                if (temSize.Width * dHeight > temSize.Width * dWidth)
                {
                    sW = dWidth;
                    sH = dWidth * temSize.Height / temSize.Width;
                }
                else
                {
                    sH = dHeight;
                    sW = temSize.Width * dHeight / temSize.Height;
                }
            }
            else
            {
                sW = temSize.Width;
                sH = temSize.Height;
            }

            using Bitmap bmpp = new Bitmap(dWidth, dHeight);
            using Graphics g = Graphics.FromImage(bmpp);
            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);

            //以下代码为保存图片时，设置压缩质量
            using var ep = new EncoderParameters();
            using var eParam = new EncoderParameter(Encoder.Quality, new long[] { quality });
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayIci = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegIcIinfo = arrayIci.FirstOrDefault(t => t.FormatDescription.Equals("JPEG"));
                if (jpegIcIinfo != null)
                {
                    bmpp.Save(dFile, jpegIcIinfo, ep);//dFile是压缩后的新路径
                    FileInfo fi = new FileInfo(dFile);
                    if (fi.Length > 1024 * size && quality > 10)
                    {
                        quality -= 10;
                        CompressImage(sFile, dFile, quality, size, false);
                    }
                }
                else
                {
                    bmpp.Save(dFile, iSource.RawFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="src">原图片文件流</param>
        /// <param name="dest">压缩后图片文件流</param>
        /// <param name="quality">压缩质量（数字越小压缩率越高）1-100</param>
        /// <param name="size">压缩后图片的最大大小</param>
        /// <param name="sfsc">是否是第一次调用</param>
        /// <returns></returns>
        public static bool CompressImage(Stream src, Stream dest, byte quality = 90, int size = 1024, bool sfsc = true)
        {
            //如果是第一次调用，原始图像的大小小于要压缩的大小，则直接复制文件，并且返回true
            if (sfsc && src.Length < size * 1024)
            {
                src.CopyTo(dest);
                return true;
            }

            using Image iSource = Image.FromStream(src);
            int dHeight = iSource.Height;
            int dWidth = iSource.Width;
            int sW, sH;

            //按比例缩放
            Size temSize = new Size(iSource.Width, iSource.Height);
            if (temSize.Width > dHeight || temSize.Width > dWidth)
            {
                if ((temSize.Width * dHeight) > (temSize.Width * dWidth))
                {
                    sW = dWidth;
                    sH = (dWidth * temSize.Height) / temSize.Width;
                }
                else
                {
                    sH = dHeight;
                    sW = (temSize.Width * dHeight) / temSize.Height;
                }
            }
            else
            {
                sW = temSize.Width;
                sH = temSize.Height;
            }

            using Bitmap bmpp = new Bitmap(dWidth, dHeight);
            using Graphics g = Graphics.FromImage(bmpp);
            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);

            //以下代码为保存图片时，设置压缩质量
            using var ep = new EncoderParameters();
            using var eParam = new EncoderParameter(Encoder.Quality, new long[] { quality });
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayIci = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegIcIinfo = arrayIci.FirstOrDefault(t => t.FormatDescription.Equals("JPEG"));
                if (jpegIcIinfo != null)
                {
                    bmpp.Save(dest, jpegIcIinfo, ep);//dFile是压缩后的新路径
                    if (dest.Length > 1024 * size && quality > 10)
                    {
                        quality -= 10;
                        CompressImage(src, dest, quality, size, false);
                    }
                }
                else
                {
                    bmpp.Save(dest, iSource.RawFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion 无损压缩图片

        #region 缩略图

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImage">原图</param>
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式</param>
        public static void MakeThumbnail(this Image originalImage, string thumbnailPath, int width, int height, ThumbnailCutMode mode)
        {
            using var bitmap = MakeThumbnail(originalImage, width, height, mode);
            bitmap.Save(thumbnailPath, originalImage.RawFormat);
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImage">原图</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式</param>
        public static Image MakeThumbnail(this Image originalImage, int width, int height, ThumbnailCutMode mode)
        {
            int towidth = width;
            int toheight = height;
            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;

            switch (mode)
            {
                case ThumbnailCutMode.Fixed: //指定高宽缩放（可能变形）
                    break;

                case ThumbnailCutMode.LockWidth: //指定宽，高按比例
                    toheight = originalImage.Height * width / originalImage.Width;
                    break;

                case ThumbnailCutMode.LockHeight: //指定高，宽按比例
                    towidth = originalImage.Width * height / originalImage.Height;
                    break;

                case ThumbnailCutMode.LockHeightAndWidth: //指定高，宽按比例
                    towidth = originalImage.Width * height / originalImage.Height;
                    towidth = towidth > width ? width : towidth;
                    toheight = originalImage.Height * towidth / originalImage.Width;
                    toheight = toheight > height ? height : toheight;
                    towidth = originalImage.Width * toheight / originalImage.Height;
                    break;

                case ThumbnailCutMode.Cut: //指定高宽裁减（不变形）
                    if (originalImage.Width / (double)originalImage.Height > towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * height / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }
                    break;
            }

            //新建一个bmpp图片
            Image bitmap = new Bitmap(towidth, toheight);

            //新建一个画板
            using Graphics g = Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分
            //第一个：对哪张图片进行操作。
            //二：画多么大。
            //三：画那块区域。
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight), new Rectangle(x, y, ow, oh), GraphicsUnit.Pixel);

            return bitmap;
        }

        #endregion 缩略图

        #region 调整光暗

        /// <summary>
        /// 调整光暗
        /// </summary>
        /// <param name="source">原始图片</param>
        /// <param name="width">原始图片的长度</param>
        /// <param name="height">原始图片的高度</param>
        /// <param name="val">增加或减少的光暗值</param>
        public static Bitmap LDPic(this Bitmap source, int width, int height, int val)
        {
            Bitmap bmp = new Bitmap(width, height); //初始化一个记录经过处理后的图片对象
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pixel = source.GetPixel(x, y);
                    var resultR = pixel.R + val; //x、y是循环次数，后面三个是记录红绿蓝三个值的
                    var resultG = pixel.G + val; //x、y是循环次数，后面三个是记录红绿蓝三个值的
                    var resultB = pixel.B + val; //x、y是循环次数，后面三个是记录红绿蓝三个值的
                    bmp.SetPixel(x, y, Color.FromArgb(resultR, resultG, resultB)); //绘图
                }
            }

            return bmp;
        }

        #endregion 调整光暗

        #region 反色处理

        /// <summary>
        /// 反色处理
        /// </summary>
        /// <param name="source">原始图片</param>
        /// <param name="width">原始图片的长度</param>
        /// <param name="height">原始图片的高度</param>
        public static Bitmap RePic(this Bitmap source, int width, int height)
        {
            var bmp = new Bitmap(width, height); //初始化一个记录处理后的图片的对象
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var pixel = source.GetPixel(x, y);
                    var resultR = 255 - pixel.R;
                    var resultG = 255 - pixel.G;
                    var resultB = 255 - pixel.B;
                    bmp.SetPixel(x, y, Color.FromArgb(resultR, resultG, resultB)); //绘图
                }
            }

            return bmp;
        }

        #endregion 反色处理

        #region 浮雕处理

        /// <summary>
        /// 浮雕处理
        /// </summary>
        /// <param name="oldBitmap">原始图片</param>
        /// <param name="width">原始图片的长度</param>
        /// <param name="height">原始图片的高度</param>
        public static Bitmap Relief(this Bitmap oldBitmap, int width, int height)
        {
            var newBitmap = new Bitmap(width, height);
            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    var color1 = oldBitmap.GetPixel(x, y);
                    var color2 = oldBitmap.GetPixel(x + 1, y + 1);
                    var r = Math.Abs(color1.R - color2.R + 128);
                    var g = Math.Abs(color1.G - color2.G + 128);
                    var b = Math.Abs(color1.B - color2.B + 128);
                    if (r > 255) r = 255;
                    if (r < 0) r = 0;
                    if (g > 255) g = 255;
                    if (g < 0) g = 0;
                    if (b > 255) b = 255;
                    if (b < 0) b = 0;
                    newBitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return newBitmap;
        }

        #endregion 浮雕处理

        #region 拉伸图片

        /// <summary>
        /// 拉伸图片
        /// </summary>
        /// <param name="bmpp">原始图片</param>
        /// <param name="newW">新的宽度</param>
        /// <param name="newH">新的高度</param>
        public static async Task<Bitmap> ResizeImageAsync(this Bitmap bmpp, int newW, int newH)
        {
            try
            {
                Bitmap bap = new Bitmap(newW, newH);
                return await Task.Run(() =>
                {
                    using Graphics g = Graphics.FromImage(bap);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bap, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, bap.Width, bap.Height), GraphicsUnit.Pixel);
                    return bap;
                }).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion 拉伸图片

        #region 滤色处理

        /// <summary>
        /// 滤色处理
        /// </summary>
        /// <param name="source">原始图片</param>
        /// <param name="width">原始图片的长度</param>
        /// <param name="height">原始图片的高度</param>
        public static Bitmap FilPic(this Bitmap source, int width, int height)
        {
            var bmp = new Bitmap(width, height);
            for (var x = 0; x < width; x++)
            {
                int y;
                for (y = 0; y < height; y++)
                {
                    var pixel = source.GetPixel(x, y);
                    bmp.SetPixel(x, y, Color.FromArgb(0, pixel.G, pixel.B)); //绘图
                }
            }

            return bmp;
        }

        #endregion 滤色处理

        #region 左右翻转

        /// <summary>
        /// 左右翻转
        /// </summary>
        /// <param name="source">原始图片</param>
        /// <param name="width">原始图片的长度</param>
        /// <param name="height">原始图片的高度</param>
        public static Bitmap RevPicLR(this Bitmap source, int width, int height)
        {
            var bmp = new Bitmap(width, height);

            //x,y是循环次数,z是用来记录像素点的x坐标的变化的
            for (var y = height - 1; y >= 0; y--)
            {
                int x; //x,y是循环次数,z是用来记录像素点的x坐标的变化的
                int z; //x,y是循环次数,z是用来记录像素点的x坐标的变化的
                for (x = width - 1, z = 0; x >= 0; x--)
                {
                    var pixel = source.GetPixel(x, y);
                    bmp.SetPixel(z++, y, Color.FromArgb(pixel.R, pixel.G, pixel.B)); //绘图
                }
            }

            return bmp;
        }

        #endregion 左右翻转

        #region 上下翻转

        /// <summary>
        /// 上下翻转
        /// </summary>
        /// <param name="source">原始图片</param>
        /// <param name="width">原始图片的长度</param>
        /// <param name="height">原始图片的高度</param>
        public static Bitmap RevPicUD(this Bitmap source, int width, int height)
        {
            var bmp = new Bitmap(width, height);
            for (var x = 0; x < width; x++)
            {
                int y;
                int z;
                for (y = height - 1, z = 0; y >= 0; y--)
                {
                    var pixel = source.GetPixel(x, y);
                    bmp.SetPixel(x, z++, Color.FromArgb(pixel.R, pixel.G, pixel.B)); //绘图
                }
            }

            return bmp;
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
            int rgb = Convert.ToInt32(0.3 * c.R + 0.59 * c.G + 0.11 * c.B);
            return Color.FromArgb(rgb, rgb, rgb);
        }

        #endregion 灰度化

        #region 转换为黑白图片

        /// <summary>
        /// 转换为黑白图片
        /// </summary>
        /// <param name="source">要进行处理的图片</param>
        /// <param name="width">图片的长度</param>
        /// <param name="height">图片的高度</param>
        public static Bitmap BWPic(this Bitmap source, int width, int height)
        {
            var bmp = new Bitmap(width, height);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var pixel = source.GetPixel(x, y);
                    var result = (pixel.R + pixel.G + pixel.B) / 3; //记录处理后的像素值
                    bmp.SetPixel(x, y, Color.FromArgb(result, result, result));
                }
            }

            return bmp;
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
            var fd = new FrameDimension(gif.FrameDimensionsList[0]);
            int count = gif.GetFrameCount(fd); //获取帧数(gif图片可能包含多帧，其它格式图片一般仅一帧)
            for (int i = 0; i < count; i++) //以Jpeg格式保存各帧
            {
                gif.SelectActiveFrame(fd, i);
                gif.Save(pSavedPath + "\\frame_" + i + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        #endregion 获取图片中的各帧

        /// <summary>
        /// 将dataUri保存为图片
        /// </summary>
        /// <param name="source">dataUri数据源</param>
        /// <returns></returns>
        /// <exception cref="Exception">操作失败。</exception>
        public static Bitmap SaveDataUriAsImageFile(this string source)
        {
            string strbase64 = source.Substring(source.IndexOf(',') + 1).Trim('\0');
            byte[] arr = Convert.FromBase64String(strbase64);
            using var ms = new MemoryStream(arr);
            using var bmpp = new Bitmap(ms);

            //新建第二个bitmap类型的bmpp2变量。
            var bmpp2 = new Bitmap(bmpp, bmpp.Width, bmpp.Height);
            using var draw = Graphics.FromImage(bmpp2);
            draw.DrawImage(bmpp, 0, 0, bmpp.Width, bmpp.Height);
            return bmpp2;
        }
    }
}
