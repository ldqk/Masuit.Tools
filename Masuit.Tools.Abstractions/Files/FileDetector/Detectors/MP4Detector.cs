using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Video)]
[FormatCategory(FormatCategory.Audio)]
[FormatCategory(FormatCategory.Image)]
internal class MP4Detector : AbstractISOBaseMediaFileDetailDetector
{
    public override string Extension => "mp4";

    protected override IEnumerable<string> NextSignature
    {
        get
        {
            yield return "isom";
        }
    }

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "MP4 Detector";
}