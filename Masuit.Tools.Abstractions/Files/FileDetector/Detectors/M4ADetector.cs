using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Audio)]
internal class M4ADetector : AbstractISOBaseMediaFileDetailDetector
{
    public override string Extension => "m4a";

    protected override IEnumerable<string> NextSignature
    {
        get
        {
            yield return "M4A ";
        }
    }

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "MP4 Contained Advanced Audio Coding(AAC) Decoder";
}
