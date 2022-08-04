using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector;

public abstract class AbstractRegexSignatureDetector : IDetector
{
    public abstract string Extension { get; }

    public virtual string Precondition => null;

    public virtual string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public virtual List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected abstract Regex Signature { get; }

    public virtual bool Detect(Stream stream)
    {
        int readBufferSize = Signature.ToString().Length * 8;
        char[] buffer = new char[readBufferSize];

        var encodings = new[]
        {
            Encoding.ASCII,
            Encoding.UTF8,
            Encoding.UTF32,
            Encoding.Unicode,
            Encoding.BigEndianUnicode
        };
        foreach (var encoding in encodings)
        {
            stream.Position = 0;
            using StreamReader reader = new StreamReader(stream, encoding, true, readBufferSize, true);
            reader.ReadBlock(buffer, 0, readBufferSize);
            if (Signature.IsMatch(new string(buffer)))
            {
                return true;
            }
        }

        return false;
    }
}