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
    private const float MmToInch = CmToInch * 0.1F;
    private const float HundredthThMmToInch = MmToInch * 0.01F;
    internal const float StandardDPI = 96f;

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
        using var br = new BinaryReader(ms);
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

    internal static bool TryGetImageBounds(ImageFormat imageFormat, MemoryStream ms, ref double width, ref double height, out double horizontalResolution, out double verticalResolution)
    {
        width = 0;
        height = 0;
        horizontalResolution = verticalResolution = StandardDPI;
        try
        {
            ms.Seek(0, SeekOrigin.Begin);
            if (imageFormat == ImageFormat.Bmp && IsBmp(ms, ref width, ref height, ref horizontalResolution, ref verticalResolution))
            {
                return true;
            }
            if (imageFormat == ImageFormat.Jpg && IsJpg(ms, ref width, ref height, ref horizontalResolution, ref verticalResolution))
            {
                return true;
            }
            if (imageFormat == ImageFormat.Gif && IsGif(ms, ref width, ref height))
            {
                return true;
            }
            if (imageFormat == ImageFormat.Png && IsPng(ms, ref width, ref height, ref horizontalResolution, ref verticalResolution))
            {
                return true;
            }
            if (imageFormat == ImageFormat.Emf && IsEmf(ms, ref width, ref height, ref horizontalResolution, ref verticalResolution))
            {
                return true;
            }
            if (imageFormat == ImageFormat.Wmf && IsWmf(ms, ref width, ref height, ref horizontalResolution, ref verticalResolution))
            {
                return true;
            }
            else if (imageFormat == ImageFormat.Svg && IsSvg(ms, ref width, ref height))
            {
                return true;
            }
            else if (imageFormat == ImageFormat.Tif && IsTif(ms, ref width, ref height, ref horizontalResolution, ref verticalResolution))
            {
                return true;
            }
            else if (imageFormat == ImageFormat.WebP && IsWebP(ms, ref width, ref height, ref horizontalResolution, ref verticalResolution))
            {
                return true;
            }
            else if (imageFormat == ImageFormat.Ico && IsIcon(ms, ref width, ref height))
            {
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
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

    private static bool IsJpg(MemoryStream ms, ref double width, ref double height, ref double horizontalResolution, ref double verticalResolution)
    {
        using (var br = new BinaryReader(ms))
        {
            if (IsJpg(br))
            {
                float xDensity = 1, yDensity = 1;
                while (ms.Position < ms.Length)
                {
                    var id = GetUInt16BigEndian(br);
                    var length = (int)GetInt16BigEndian(br);
                    switch (id)
                    {
                        case 0xFFE0:
                            var identifier = br.ReadBytes(5); //JFIF\0
                            var version = br.ReadBytes(2);
                            var unit = br.ReadByte();
                            xDensity = (int)GetInt16BigEndian(br);
                            yDensity = (int)GetInt16BigEndian(br);

                            if (unit == 1)
                            {
                                horizontalResolution = xDensity;
                                verticalResolution = yDensity;
                            }
                            else if (unit == 2)
                            {
                                horizontalResolution = xDensity * CmToInch;
                                verticalResolution = yDensity * CmToInch;
                            }

                            ms.Position += length - 14;
                            break;

                        case 0xFFE1:
                            var pos = ms.Position;
                            identifier = br.ReadBytes(6); //EXIF\0\0 or //EXIF\FF\FF
                            double w = 0, h = 0;
                            ReadTiffHeader(br, ref w, ref h, ref horizontalResolution, ref verticalResolution);
                            ms.Position = pos + length - 2;
                            break;

                        case 0xFFC0:
                        case 0xFFC1:
                        case 0xFFC2:
                            var precision = br.ReadByte(); //Bits
                            height = GetUInt16BigEndian(br);
                            width = GetUInt16BigEndian(br);
                            br.Close();
                            return true;

                        case 0xFFD9:
                            return height != 0 && width != 0;

                        default:
                            ms.Position += length - 2;
                            break;
                    }
                }
            }
            return false;
        }
    }

    private static bool IsJpg(BinaryReader br)
    {
        br.BaseStream.Position = 0;
        var sign = br.ReadBytes(2);    //FF D8
        return sign.Length >= 2 && sign[0] == 0xFF && sign[1] == 0xD8;
    }

    private static bool IsGif(MemoryStream ms, ref double width, ref double height)
    {
        using (var br = new BinaryReader(ms))
        {
            if (IsGif(br))
            {
                width = br.ReadUInt16();
                height = br.ReadUInt16();
                br.Close();
                return true;
            }
        }
        return false;
    }

    private static bool IsGif(BinaryReader br)
    {
        br.BaseStream.Seek(0, SeekOrigin.Begin);
        var b = br.ReadBytes(6);
        return b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46;    //byte 4-6 contains the version, but we don't check them here.
    }

    private static bool IsBmp(MemoryStream ms, ref double width, ref double height, ref double horizontalResolution, ref double verticalResolution)
    {
        using (var br = new BinaryReader(ms))
        {
            if (IsBmp(br, out string sign))
            {
                var size = br.ReadInt32();
                var reserved = br.ReadBytes(4);
                var offsetData = br.ReadInt32();

                //Info Header
                var ihSize = br.ReadInt32(); //Should be 40
                width = br.ReadInt32();
                height = br.ReadInt32();

                if (sign == "BM")
                {
                    br.ReadBytes(12);
                    horizontalResolution = br.ReadInt32() / MToInch;
                    verticalResolution = br.ReadInt32() / MToInch;
                }
                else
                {
                    horizontalResolution = verticalResolution = 1;
                }

                return true;
            }
        }
        return false;
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

    private static bool IsIcon(MemoryStream ms, ref double width, ref double height)
    {
        using (var br = new BinaryReader(ms))
        {
            if (IsIco(br))
            {
                var imageCount = br.ReadInt16();
                width = br.ReadByte();
                if (width == 0) width = 256;
                height = br.ReadByte();
                if (height == 0) height = 256;
                br.Close();
                return true;
            }
            br.Close();
            return false;
        }
    }

    internal static bool IsIco(BinaryReader br)
    {
        br.BaseStream.Seek(0, SeekOrigin.Begin);
        var type0 = br.ReadInt16();
        var type1 = br.ReadInt16();
        return type0 == 0 && type1 == 1;
    }

    #endregion Ico

    #region WebP

    private static bool IsWebP(MemoryStream ms, ref double width, ref double height, ref double horizontalResolution, ref double verticalResolution)
    {
        width = height = 0;
        horizontalResolution = verticalResolution = StandardDPI * (1 + 1 / 3); //Excel seems to render webp at 1 1/3 size.
        using (var br = new BinaryReader(ms))
        {
            if (IsWebP(br))
            {
                var vp8 = Encoding.ASCII.GetString(br.ReadBytes(4));
                switch (vp8)
                {
                    case "VP8 ":
                        var b = br.ReadBytes(10);
                        var w = br.ReadInt16();
                        width = w & 0x3FFF;
                        var hScale = w >> 14;
                        var h = br.ReadInt16();
                        height = h & 0x3FFF;
                        hScale = h >> 14;
                        break;

                    case "VP8X":
                        br.ReadBytes(8);
                        b = br.ReadBytes(6);
                        width = BitConverter.ToInt32(new byte[] { b[0], b[1], b[2], 0 }, 0) + 1;
                        height = BitConverter.ToInt32(new byte[] { b[3], b[4], b[5], 0 }, 0) + 1;
                        break;

                    case "VP8L":
                        br.ReadBytes(5);
                        b = br.ReadBytes(4);
                        width = (b[0] | (b[1] & 0x3F) << 8) + 1;
                        height = (b[1] >> 6 | b[2] << 2 | (b[3] & 0x0F) << 10) + 1;
                        break;
                }
            }
        }
        return width != 0 && height != 0;
    }

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

    private static bool IsTif(MemoryStream ms, ref double width, ref double height, ref double horizontalResolution, ref double verticalResolution)
    {
        using (var br = new BinaryReader(ms))
        {
            return ReadTiffHeader(br, ref width, ref height, ref horizontalResolution, ref verticalResolution);
        }
    }

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
        else
        {
            return br.ReadInt16();
        }
    }

    private static int GetTifInt32(BinaryReader br, bool isBigEndian)
    {
        if (isBigEndian)
        {
            return GetInt32BigEndian(br);
        }
        else
        {
            return br.ReadInt32();
        }
    }

    #endregion Tiff

    #region Emf

    private static bool IsEmf(MemoryStream ms, ref double width, ref double height, ref double horizontalResolution, ref double verticalResolution)
    {
        using (var br = new BinaryReader(ms))
        {
            if (IsEmf(br))
            {
                var length = br.ReadInt32();
                var bounds = new int[4];
                bounds[0] = br.ReadInt32();
                bounds[1] = br.ReadInt32();
                bounds[2] = br.ReadInt32();
                bounds[3] = br.ReadInt32();
                var frame = new int[4];
                frame[0] = br.ReadInt32();
                frame[1] = br.ReadInt32();
                frame[2] = br.ReadInt32();
                frame[3] = br.ReadInt32();

                var signatureBytes = br.ReadBytes(4);
                var signature = Encoding.ASCII.GetString(signatureBytes);
                if (signature.Trim() == "EMF")
                {
                    var version = br.ReadUInt32();
                    var size = br.ReadUInt32();
                    var records = br.ReadUInt32();
                    var handles = br.ReadUInt16();
                    var reserved = br.ReadUInt16();

                    var nDescription = br.ReadUInt32();
                    var offDescription = br.ReadUInt32();
                    var nPalEntries = br.ReadUInt32();
                    var device = new uint[2];
                    device[0] = br.ReadUInt32();
                    device[1] = br.ReadUInt32();

                    var mm = new uint[2];
                    mm[0] = br.ReadUInt32();
                    mm[1] = br.ReadUInt32();

                    //Extension 1
                    var cbPixelFormat = br.ReadUInt32();
                    var offPixelFormat = br.ReadUInt32();
                    var bOpenGL = br.ReadUInt32();

                    //Extension 2
                    var hr = br.ReadUInt32();
                    var vr = br.ReadUInt32();

                    var id = br.ReadInt32();
                    var size2 = br.ReadInt32();

                    width = (bounds[2] - bounds[0] + 1);
                    height = (bounds[3] - bounds[1] + 1);

                    horizontalResolution = width / ((frame[2] - frame[0]) * HundredthThMmToInch * StandardDPI) * StandardDPI;
                    verticalResolution = height / ((frame[3] - frame[1]) * HundredthThMmToInch * StandardDPI) * StandardDPI;

                    return true;
                }
            }
        }
        return false;
    }

    private static bool IsEmf(BinaryReader br)
    {
        br.BaseStream.Position = 0;
        var type = br.ReadInt32();
        return type == 1;
    }

    #endregion Emf

    #region Wmf

    private const double PIXELS_PER_TWIPS = 1D / 15D;
    private const double DEFAULT_TWIPS = 1440D;

    private static bool IsWmf(MemoryStream ms, ref double width, ref double height, ref double horizontalResolution, ref double verticalResolution)
    {
        using (var br = new BinaryReader(ms))
        {
            if (IsWmf(br))
            {
                var HWmf = br.ReadInt16();
                var bounds = new ushort[4];
                bounds[0] = br.ReadUInt16();
                bounds[1] = br.ReadUInt16();
                bounds[2] = br.ReadUInt16();
                bounds[3] = br.ReadUInt16();

                var inch = br.ReadInt16();
                width = bounds[2] - bounds[0];
                height = bounds[3] - bounds[1];
                if (inch != 0)
                {
                    width *= (DEFAULT_TWIPS / inch) * PIXELS_PER_TWIPS;
                    height *= (DEFAULT_TWIPS / inch) * PIXELS_PER_TWIPS;
                }
                return width != 0 && height != 0;
            }
        }
        return false;
    }

    private static bool IsWmf(BinaryReader br)
    {
        br.BaseStream.Position = 0;
        var key = br.ReadUInt32();
        return key == 0x9AC6CDD7;
    }

    #endregion Wmf

    #region Png

    private static bool IsPng(MemoryStream ms, ref double width, ref double height, ref double horizontalResolution, ref double verticalResolution)
    {
        using (var br = new BinaryReader(ms))
        {
            return IsPng(br, ref width, ref height, ref horizontalResolution, ref verticalResolution);
        }
    }

    private static bool IsPng(BinaryReader br, ref double width, ref double height, ref double horizontalResolution, ref double verticalResolution, long fileEndPosition = long.MinValue)
    {
        if (IsPng(br))
        {
            if (fileEndPosition == long.MinValue)
            {
                fileEndPosition = br.BaseStream.Length;
            }
            while (br.BaseStream.Position < fileEndPosition)
            {
                var chunkType = ReadPngChunkHeader(br, out int length);
                switch (chunkType)
                {
                    case "IHDR":
                        width = GetInt32BigEndian(br);
                        height = GetInt32BigEndian(br);
                        br.ReadBytes(5); //Ignored bytes, Depth compression etc.
                        break;

                    case "pHYs":
                        horizontalResolution = GetInt32BigEndian(br);
                        verticalResolution = GetInt32BigEndian(br);
                        var unitSpecifier = br.ReadByte();
                        if (unitSpecifier == 1)
                        {
                            horizontalResolution /= MToInch;
                            verticalResolution /= MToInch;
                        }

                        br.Close();
                        return true;

                    default:
                        br.ReadBytes(length);
                        break;
                }
                var crc = br.ReadInt32();
            }
        }
        br.Close();
        return width != 0 && height != 0;
    }

    private static bool IsPng(BinaryReader br)
    {
        br.BaseStream.Position = 0;
        var signature = br.ReadBytes(8);
        return signature.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
    }

    private static string ReadPngChunkHeader(BinaryReader br, out int length)
    {
        length = GetInt32BigEndian(br);
        var b = br.ReadBytes(4);
        var type = Encoding.ASCII.GetString(b);
        return type;
    }

    #endregion Png

    #region Svg

    private static bool IsSvg(MemoryStream ms, ref double width, ref double height)
    {
        try
        {
            var reader = new XmlTextReader(ms);
            while (reader.Read())
            {
                if (reader.LocalName == "svg" && reader.NodeType == XmlNodeType.Element)
                {
                    var w = reader.GetAttribute("width");
                    var h = reader.GetAttribute("height");
                    var vb = reader.GetAttribute("viewBox");
                    reader.Close();
                    if (w == null || h == null)
                    {
                        if (vb == null)
                        {
                            return false;
                        }
                        var bounds = vb.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (bounds.Length < 4)
                        {
                            return false;
                        }
                        if (string.IsNullOrEmpty(w))
                        {
                            w = bounds[2];
                        }
                        if (string.IsNullOrEmpty(h))
                        {
                            h = bounds[3];
                        }
                    }
                    width = GetSvgUnit(w);
                    if (double.IsNaN(width)) return false;
                    height = GetSvgUnit(h);
                    if (double.IsNaN(height)) return false;
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

    private static double GetSvgUnit(string v)
    {
        var factor = 1D;
        if (v.EndsWith("px", StringComparison.OrdinalIgnoreCase))
        {
            v = v.Substring(0, v.Length - 2);
        }
        else if (v.EndsWith("pt", StringComparison.OrdinalIgnoreCase))
        {
            factor = 1.25;
            v = v.Substring(0, v.Length - 2);
        }
        else if (v.EndsWith("pc", StringComparison.OrdinalIgnoreCase))
        {
            factor = 15;
            v = v.Substring(0, v.Length - 2);
        }
        else if (v.EndsWith("mm", StringComparison.OrdinalIgnoreCase))
        {
            factor = 3.543307;
            v = v.Substring(0, v.Length - 2);
        }
        else if (v.EndsWith("cm", StringComparison.OrdinalIgnoreCase))
        {
            factor = 35.43307;
            v = v.Substring(0, v.Length - 2);
        }
        else if (v.EndsWith("in", StringComparison.OrdinalIgnoreCase))
        {
            factor = 90;
            v = v.Substring(0, v.Length - 2);
        }
        if (double.TryParse(v, out double value))
        {
            return value * factor;
        }
        return double.NaN;
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
