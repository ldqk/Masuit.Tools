using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Video)]
[FormatCategory(FormatCategory.Audio)]
internal sealed class QuickTimeDetector : AbstractISOBaseMediaFileDetailDetector
{
    public override string Extension => "mov";

    protected override IEnumerable<string> NextSignature
    {
        get
        {
            yield return "qt";
        }
    }

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "QuickTime(MOV) Detector";

    public override bool Detect(Stream stream)
    {
        if (!base.Detect(stream))
        {
            stream.Position = 4;
            return stream.ReadByte() == 0x6D && stream.ReadByte() == 0x6F && stream.ReadByte() == 0x6F && stream.ReadByte() == 0x76;
        }

        return true;
    }
}
