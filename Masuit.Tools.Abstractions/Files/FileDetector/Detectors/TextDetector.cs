using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Document)]
internal class TextDetector : IDetector
{
    private static readonly byte[] SignatureBuffer = new byte[4];
    private static readonly char[] TextBuffer = new char[4096];
    private static readonly byte[] ReadBuffer = new byte[4096];
    private static readonly byte[] EncodingBuffer = new byte[4096];
    private static readonly Encoding[] Utf8Encodings = { Encoding.UTF8 };
    private static readonly Encoding[] Utf16Encodings = { Encoding.Unicode };
    private static readonly Encoding[] Utf16BeEncodings = { Encoding.BigEndianUnicode };
    private static readonly Encoding[] Utf32Encodings = { Encoding.GetEncoding("utf-32") };

    private static readonly Encoding[] OtherwiseEncodings = {
        Encoding.GetEncoding ( "ascii" ),
        Encoding.UTF8,
        Encoding.GetEncoding ( "utf-32" ),
        Encoding.Unicode,
        Encoding.BigEndianUnicode
    };

    public string Extension => "txt";

    public string Precondition => null;

    public string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public bool Detect(Stream stream)
    {
        _ = stream.Read(SignatureBuffer, 0, SignatureBuffer.Length);
        stream.Seek(0, SeekOrigin.Begin);

        Encoding[] encodings;

        if (SignatureBuffer[0] == 0xef && SignatureBuffer[1] == 0xbb && SignatureBuffer[2] == 0xbf)
        {
            encodings = Utf8Encodings;
            stream.Position = 3;
        }
        else if (SignatureBuffer[0] == 0xfe && SignatureBuffer[1] == 0xff)
        {
            encodings = Utf16Encodings;
            stream.Position = 2;
        }
        else if (SignatureBuffer[0] == 0xff && SignatureBuffer[1] == 0xfe)
        {
            encodings = Utf16BeEncodings;
            stream.Position = 2;
        }
        else if (SignatureBuffer[0] == 0 && SignatureBuffer[1] == 0 && SignatureBuffer[2] == 0xfe && SignatureBuffer[3] == 0xff)
        {
            encodings = Utf32Encodings;
            stream.Position = 4;
        }
        else
        {
            encodings = OtherwiseEncodings;
            stream.Position = 0;
        }

        int readed = stream.Read(ReadBuffer, 0, /*2048*/1024);

        foreach (var encoding in encodings)
        {
            for (int count = readed; count >= (readed - 16); --count)
            {
                bool succeed = true;
                int texted = encoding.GetChars(ReadBuffer, 0, count, TextBuffer, 0);
                for (int i = 0; i < texted; ++i)
                {
                    char ch = TextBuffer[i];
                    if ((char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t') || ch == '\0')
                    {
                        succeed = false;
                        break;
                    }
                }

                _ = encoding.GetBytes(TextBuffer, 0, texted, EncodingBuffer, 0);
                if (succeed/* && readed == byted*/ )
                {
                    for (int i = 0; i < count; ++i)
                    {
                        if (ReadBuffer[i] != EncodingBuffer[i])
                        {
                            succeed = false;
                            break;
                        }
                    }
                }
                else
                {
                    continue;
                }

                if (succeed)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override string ToString() => "Text File Detector";
}
