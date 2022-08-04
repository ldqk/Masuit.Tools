using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.System)]
[FormatCategory(FormatCategory.Executable)]
internal class DLLDetector : IDetector
{
    public string Precondition => "exe";

    public string Extension => "dll";

    public string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public bool Detect(Stream stream)
    {
        stream.Position = 60;
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        stream.Position = reader.ReadInt32() + 4 + 18;
        short characteristics = reader.ReadInt16();
        return (characteristics & 0x2000) != 0;
    }

    public override string ToString() => "Windows Dynamic Linkage Library Detector";
}
