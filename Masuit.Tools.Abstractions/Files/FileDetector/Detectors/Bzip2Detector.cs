using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Compression)]
internal sealed class Bzip2Detector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] Bz2SignatureInfo = {
        new() { Position = 0, Signature = "BZh"u8.ToArray() },
    };

    public override string Extension => "bz2";

    protected override SignatureInformation[] SignatureInformations => Bz2SignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Bunzip2 Detector";
}
