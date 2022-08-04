using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Video)]
[FormatCategory(FormatCategory.Audio)]
internal class _3GPDetector : AbstractISOBaseMediaFileDetailDetector
{
    public override string Extension => "3gp";

    protected override IEnumerable<string> NextSignature
    {
        get
        {
            yield return "3gp";
        }
    }

    public override string ToString() => "3GPP Detector";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();
}
