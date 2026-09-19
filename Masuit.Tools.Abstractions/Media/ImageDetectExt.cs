using Masuit.Tools.Systems;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace Masuit.Tools.Media;

public static class ImageDetectExt
{
    public static bool IsImage(this FileInfo file)
    {
        using var stream = file.OpenRead();
        return IsImage(stream);
    }

    public static bool IsImage(this Stream s)
    {
        return GetImageType(s) != null;
    }

    /// <summary>
    /// 获取图像格式（通过魔法字节检测）
    /// </summary>
    public static ImageFormat? GetImageType(this Stream ms)
    {
        ms.Seek(0, SeekOrigin.Begin);
        var header = new byte[18];
        int read = ms.Read(header, 0, header.Length);
        ms.Seek(0, SeekOrigin.Begin);

        if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return ImageFormat.Jpg;

        if (read >= 8 && header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x00 && header[3] == 0x0C
            && header[4] == 0x6A && header[5] == 0x50 && header[6] == 0x20 && header[7] == 0x20)
            return ImageFormat.Jpeg2000;

        if (read >= 4 && header[0] == 0x49 && header[1] == 0x49 && header[2] == 0xBC && header[3] == 0x01)
            return ImageFormat.JpegXR;

        if (read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
            && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            return ImageFormat.Png;

        if (read >= 4 && header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
            return ImageFormat.Gif;

        if (read >= 2 && header[0] == 0x42 && header[1] == 0x4D)
            return ImageFormat.Bmp;

        if (read >= 4 && ((header[0] == 0x49 && header[1] == 0x49 && header[2] == 0x2A && header[3] == 0x00)
                          || (header[0] == 0x4D && header[1] == 0x4D && header[2] == 0x00 && header[3] == 0x2A)))
            return ImageFormat.Tif;

        if (read >= 18 && IsTga(header))
            return ImageFormat.Tga;

        if (read >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            return ImageFormat.WebP;

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
            _ = ExtractImage(ms.ToArray(), out ImageFormat? pt);
            ms.Seek(0, SeekOrigin.Begin);
            return pt;
        }

        return null;
    }

    private static bool IsTga(byte[] header)
    {
        int imageType = header[2];
        int colorMapType = header[1];
        int colorDepth = header[17];
        return imageType is 0 or 1 or 2 or 3 or 9 or 10 or 11 or 32 or 33
               && colorDepth is 8 or 15 or 16 or 24 or 32
               && (colorMapType == 0 || (colorMapType == 1 && colorDepth == 8));
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
                    if (size > 0) msOut.Write(buffer, 0, size);
                } while (size == bufferSize);

                msOut.Position = 0;
                var br = new BinaryReader(msOut);
                if (IsEmf(br)) type = ImageFormat.Emf;
                else if (IsWmf(br)) type = ImageFormat.Wmf;
                else type = null;

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

    internal static bool IsIco(BinaryReader br)
    {
        br.BaseStream.Seek(0, SeekOrigin.Begin);
        var type0 = br.ReadInt16();
        var type1 = br.ReadInt16();
        return type0 == 0 && type1 == 1;
    }

    private static bool IsEmf(BinaryReader br)
    {
        br.BaseStream.Position = 0;
        var type = br.ReadInt32();
        return type == 1;
    }

    private static bool IsWmf(BinaryReader br)
    {
        br.BaseStream.Position = 0;
        var key = br.ReadUInt32();
        return key == 0x9AC6CDD7;
    }

    private static bool IsSvg(Stream ms)
    {
        try
        {
            ms.Position = 0;
            var reader = new XmlTextReader(ms);
            while (reader.Read())
            {
                if (reader.LocalName == "svg" && reader.NodeType == System.Xml.XmlNodeType.Element)
                    return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}
