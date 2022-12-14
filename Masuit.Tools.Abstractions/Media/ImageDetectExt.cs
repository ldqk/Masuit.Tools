using Masuit.Tools.Systems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace Masuit.Tools.Media;

public static class ImageDetectExt
{
    public static bool IsImage(this Stream s)
    {
        return GetImageType(s) != null;
    }

    /// <summary>
    /// 获取图像格式
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    public static ImageFormat? GetImageType(this Stream ms)
    {
        ms.Seek(0, SeekOrigin.Begin);
        var pictureType = Image.DetectFormat(ms) switch
        {
            BmpFormat => ImageFormat.Bmp,
            GifFormat => ImageFormat.Gif,
            JpegFormat => ImageFormat.Jpg,
            PngFormat => ImageFormat.Png,
            TiffFormat => ImageFormat.Tif,
            WebpFormat => ImageFormat.WebP,
            _ => new ImageFormat?()
        };
        if (pictureType.HasValue)
        {
            ms.Seek(0, SeekOrigin.Begin);
            return pictureType;
        }

        var br = new BinaryReader(ms);
        if (IsIco(br))
        {
            ms.Seek(0, SeekOrigin.Begin);
            return ImageFormat.Ico;
        }

        if (IsEmf(br))
        {
            ms.Seek(0, SeekOrigin.Begin);
            return ImageFormat.Emf;
        }

        if (IsWmf(br))
        {
            ms.Seek(0, SeekOrigin.Begin);
            return ImageFormat.Wmf;
        }

        if (IsSvg(ms))
        {
            ms.Seek(0, SeekOrigin.Begin);
            return ImageFormat.Svg;
        }

        if (IsGZip(br))
        {
            _ = ExtractImage(ToArray(ms), out ImageFormat? pt);
            ms.Seek(0, SeekOrigin.Begin);
            return pt;
        }

        return null;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static byte[] ToArray(Stream stream)
    {
        stream.Position = 0;
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);

        // 设置当前流的位置为流的开始
        stream.Seek(0, SeekOrigin.Begin);
        return bytes;
    }

    private static bool IsGZip(BinaryReader br)
    {
        br.BaseStream.Position = 0;
        var sign = br.ReadBytes(2);
        return IsGZip(sign);
    }

    private static bool IsGZip(byte[] sign)
    {
        return sign.Length >= 2 && sign[0] == 0x1F && sign[1] == 0x8B;
    }

    internal static byte[] ExtractImage(byte[] img, out ImageFormat? type)
    {
        if (IsGZip(img))
        {
            try
            {
                using var ms = new PooledMemoryStream(img);
                using var msOut = new PooledMemoryStream();
                const int bufferSize = 4096;
                var buffer = new byte[bufferSize];
                using var z = new GZipStream(ms, CompressionMode.Decompress);
                int size = 0;
                do
                {
                    size = z.Read(buffer, 0, bufferSize);
                    if (size > 0)
                    {
                        msOut.Write(buffer, 0, size);
                    }
                }
                while (size == bufferSize);
                msOut.Position = 0;
                var br = new BinaryReader(msOut);
                if (IsEmf(br))
                {
                    type = ImageFormat.Emf;
                }
                else if (IsWmf(br))
                {
                    type = ImageFormat.Wmf;
                }
                else
                {
                    type = null;
                }
                msOut.Position = 0;
                return msOut.ToArray();
            }
            catch
            {
                type = null;
                return img;
            }
        }
        type = null;
        return img;
    }

    #region Ico

    internal static bool IsIco(BinaryReader br)
    {
        br.BaseStream.Seek(0, SeekOrigin.Begin);
        var type0 = br.ReadInt16();
        var type1 = br.ReadInt16();
        return type0 == 0 && type1 == 1;
    }

    #endregion Ico

    #region Emf

    private static bool IsEmf(BinaryReader br)
    {
        br.BaseStream.Position = 0;
        var type = br.ReadInt32();
        return type == 1;
    }

    #endregion Emf

    #region Wmf

    private static bool IsWmf(BinaryReader br)
    {
        br.BaseStream.Position = 0;
        var key = br.ReadUInt32();
        return key == 0x9AC6CDD7;
    }

    #endregion Wmf

    #region Svg

    private static bool IsSvg(Stream ms)
    {
        try
        {
            ms.Position = 0;
            var reader = new XmlTextReader(ms);
            while (reader.Read())
            {
                if (reader.LocalName == "svg" && reader.NodeType == XmlNodeType.Element)
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    #endregion Svg
}
