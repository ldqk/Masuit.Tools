using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Archive)]
internal class POSIXtarDetector : IDetector
{
    public string Precondition => null;

    public string Extension => "tar";

    public bool Detect(Stream stream)
    {
        stream.Position = 100;
        byte[] mode = new byte[8];
        stream.Read(mode, 0, 8);
        return Regex.IsMatch(Encoding.GetEncoding("ascii").GetString(mode, 0, 8), "[0-7][0-7][0-7][0-7][0-7][0-7][0-7][\0]");
    }

    public string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "pre-POSIX Tar(TAR) Detector";
}
