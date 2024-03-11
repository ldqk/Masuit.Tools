using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Masuit.Tools.Abstractions.Files;

/// <summary>
/// 字节文本编码检测
/// </summary>
public static class TextEncodingDetector
{
    /// <summary>
    /// 检测文本文件编码
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static Encoding GetEncoding(string file)
    {
        return GetEncoding(new FileInfo(file));
    }

    /// <summary>
    /// 检测文本文件编码
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static Encoding GetEncoding(this FileInfo file)
    {
        using var fs = file.OpenRead();
        return GetEncoding(fs);
    }

    /// <summary>
    /// 检测文本流编码
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static Encoding GetEncoding(this Stream stream)
    {
        var bytes = new byte[4];
        _ = stream.Read(bytes, 0, 4);
        return GetEncoding(bytes);
    }

    private static Encoding GetEncoding(IReadOnlyList<byte> bytes)
    {
        if (bytes.Count < 4)
        {
            throw new ArgumentException("Byte array is too short");
        }

        if (bytes[0] == 255 && bytes[1] == 254 && bytes[2] == 0 && bytes[3] == 0)
        {
            return Encoding.UTF32;
        }
        if (bytes[0] == 254 && bytes[1] == 255 && bytes[2] == 0)
        {
            return Encoding.BigEndianUnicode;
        }
        if (bytes[0] == 255 && bytes[1] == 254)
        {
            return Encoding.Unicode;
        }
        if (bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 254 && bytes[3] == 255)
        {
            return Encoding.GetEncoding("utf-32BE");
        }
        if (bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191)
        {
            return Encoding.UTF8;
        }

        return Encoding.ASCII;
    }
}