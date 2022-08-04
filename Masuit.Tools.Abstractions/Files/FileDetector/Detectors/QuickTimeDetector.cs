using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Video)]
[FormatCategory(FormatCategory.Audio)]
internal class QuickTimeDetector : AbstractISOBaseMediaFileDetailDetector
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
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            return buffer[0] == 0x6D && buffer[1] == 0x6F && buffer[2] == 0x6F && buffer[3] == 0x76;
        }

        return true;
    }
}
