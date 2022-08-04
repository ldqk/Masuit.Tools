using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Video)]
internal class M4VDetector : AbstractISOBaseMediaFileDetailDetector
{
    public override string Extension => "m4v";

    protected override IEnumerable<string> NextSignature
    {
        get
        {
            yield return "mp42";
        }
    }

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "MP4 Contained H.264(AVC) Decoder";
}
