using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace Masuit.Tools.Media;

public static class ImageDetectExt
{
    private const float MToInch = 39.3700787F;
    private const float CmToInch = MToInch * 0.01F;

    internal struct TifIfd
    {
        public short Tag;
        public short Type;
        public int Count;
        public int ValueOffset;
    }

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
        var br = new BinaryReader(ms);
        if (IsJpg(br))
        {
            return ImageFormat.Jpg;
        }
        if (IsBmp(br, out _))
        {
            return ImageFormat.Bmp;
        }

        if (IsGif(br))
        {
            return ImageFormat.Gif;
        }

        if (IsPng(br))
        {
            return ImageFormat.Png;
        }

        if (IsTif(br, out _))
        {
            return ImageFormat.Tif;
        }

        if (IsIco(br))
        {
            return ImageFormat.Ico;
        }

        if (IsWebP(br))
        {
            return ImageFormat.WebP;
        }

        if (IsEmf(br))
        {
            return ImageFormat.Emf;
        }

        if (IsWmf(br))
        {
            return ImageFormat.Wmf;
        }

        if (IsSvg(ms))
        {
            return ImageFormat.Svg;
        }

        if (IsGZip(br))
        {
            _ = ExtractImage(ToArray(ms), out ImageFormat? pt);
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
                var ms = new MemoryStream(img);
                var msOut = new MemoryStream();
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

    private static bool IsJpg(BinaryReader br)
    {
        br.BaseStream.Position = 0;
        var sign = br.ReadBytes(2);    //FF D8
        return sign.Length >= 2 && sign[0] == 0xFF && sign[1] == 0xD8;
    }

    private static bool IsGif(BinaryReader br)
    {
        br.BaseStream.Seek(0, SeekOrigin.Begin);
        var b = br.ReadBytes(6);
        return b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46;    //byte 4-6 contains the version, but we don't check them here.
    }

    internal static bool IsBmp(BinaryReader br, out string sign)
    {
        try
        {
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            sign = Encoding.ASCII.GetString(br.ReadBytes(2));    //BM for a Windows bitmap
            return (sign == "BM" || sign == "BA" || sign == "CI" || sign == "CP" || sign == "IC" || sign == "PT");
        }
        catch
        {
            sign = null;
            return false;
        }
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

    #region WebP

    internal static bool IsWebP(BinaryReader br)
    {
        try
        {
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            var riff = Encoding.ASCII.GetString(br.ReadBytes(4));
            var length = GetInt32BigEndian(br);
            var webP = Encoding.ASCII.GetString(br.ReadBytes(4));

            return riff == "RIFF" && webP == "WEBP";
        }
        catch
        {
            return false;
        }
    }

    #endregion WebP

    #region Tiff

    private static bool ReadTiffHeader(BinaryReader br, ref double width, ref double height, ref double horizontalResolution, ref double verticalResolution)
    {
        var ms = br.BaseStream;
        var pos = ms.Position;
        if (IsTif(br, out bool isBigEndian, false))
        {
            var offset = GetTifInt32(br, isBigEndian);
            ms.Position = pos + offset;
            var numberOfIdf = GetTifInt16(br, isBigEndian);
            var ifds = new List<TifIfd>();
            for (int i = 0; i < numberOfIdf; i++)
            {
                var ifd = new TifIfd()
                {
                    Tag = GetTifInt16(br, isBigEndian),
                    Type = GetTifInt16(br, isBigEndian),
                    Count = GetTifInt32(br, isBigEndian),
                };
                if (ifd.Type == 1 || ifd.Type == 2 || ifd.Type == 6 || ifd.Type == 7)
                {
                    ifd.ValueOffset = br.ReadByte();
                    br.ReadBytes(3);
                }
                else if (ifd.Type == 3 || ifd.Type == 8)
                {
                    ifd.ValueOffset = GetTifInt16(br, isBigEndian);
                    br.ReadBytes(2);
                }
                else
                {
                    ifd.ValueOffset = GetTifInt32(br, isBigEndian);
                }
                ifds.Add(ifd);
            }

            int resolutionUnit = 2;
            foreach (var ifd in ifds)
            {
                switch (ifd.Tag)
                {
                    case 0x100:
                        width = ifd.ValueOffset;
                        break;

                    case 0x101:
                        height = ifd.ValueOffset;
                        break;

                    case 0x11A:
                        ms.Position = ifd.ValueOffset + pos;
                        var l1 = GetTifInt32(br, isBigEndian);
                        var l2 = GetTifInt32(br, isBigEndian);
                        horizontalResolution = l1 / l2;
                        break;

                    case 0x11B:
                        ms.Position = ifd.ValueOffset + pos;
                        l1 = GetTifInt32(br, isBigEndian);
                        l2 = GetTifInt32(br, isBigEndian);
                        verticalResolution = l1 / l2;
                        break;

                    case 0x128:
                        resolutionUnit = ifd.ValueOffset;
                        break;
                }
            }
            if (resolutionUnit == 1)
            {
                horizontalResolution *= CmToInch;
                verticalResolution *= CmToInch;
            }
        }
        return width != 0 && height != 0;
    }

    private static bool IsTif(BinaryReader br, out bool isBigEndian, bool resetPos = false)
    {
        try
        {
            if (resetPos)
            {
                br.BaseStream.Position = 0;
            }
            var b = br.ReadBytes(2);
            isBigEndian = Encoding.ASCII.GetString(b) == "MM";
            var identifier = GetTifInt16(br, isBigEndian);
            if (identifier == 42)
            {
                return true;
            }
        }
        catch
        {
            isBigEndian = false;
            return false;
        }
        return false;
    }

    private static short GetTifInt16(BinaryReader br, bool isBigEndian)
    {
        if (isBigEndian)
        {
            return GetInt16BigEndian(br);
        }

        return br.ReadInt16();
    }

    private static int GetTifInt32(BinaryReader br, bool isBigEndian)
    {
        if (isBigEndian)
        {
            return GetInt32BigEndian(br);
        }

        return br.ReadInt32();
    }

    #endregion Tiff

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

    #region Png

    private static bool IsPng(BinaryReader br)
    {
        br.BaseStream.Position = 0;
        var signature = br.ReadBytes(8);
        return signature.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
    }

    #endregion Png

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

    private static ushort GetUInt16BigEndian(BinaryReader br)
    {
        var b = br.ReadBytes(2);
        return BitConverter.ToUInt16(new byte[] { b[1], b[0] }, 0);
    }

    private static short GetInt16BigEndian(BinaryReader br)
    {
        var b = br.ReadBytes(2);
        return BitConverter.ToInt16(new byte[] { b[1], b[0] }, 0);
    }

    private static int GetInt32BigEndian(BinaryReader br)
    {
        var b = br.ReadBytes(4);
        return BitConverter.ToInt32(new byte[] { b[3], b[2], b[1], b[0] }, 0);
    }
}
