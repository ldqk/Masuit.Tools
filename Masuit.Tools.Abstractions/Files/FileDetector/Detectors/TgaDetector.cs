using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Image)]
internal sealed class TgaDetector : IDetector
{
    public string Extension => "tga";

    public string Precondition => null;

    public string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public bool Detect(Stream stream)
    {
        stream.Position = 3;
        int compressionType = stream.ReadByte();
        if (compressionType is not (0 or 1 or 2 or 3 or 9 or 10 or 11 or 32 or 33))
        {
            return false;
        }

        stream.Position = 17;
        int depth = stream.ReadByte();
        if (depth is not (8 or 24 or 15 or 16 or 32))
        {
            return false;
        }

        stream.Position = 1;
        int mapType = stream.ReadByte();
        return mapType == 0 || (mapType == 1 && depth == 8);
    }

    public override string ToString() => "Targa Detector (Experimental)";
}
