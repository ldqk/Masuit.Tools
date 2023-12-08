using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Archive)]
internal sealed class CabDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] CabSignatureInfo = {
        new() { Position = 0, Signature = "MSCF"u8.ToArray() },
    };

    public override string Extension => "cab";

    protected override SignatureInformation[] SignatureInformations => CabSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Windows Cabinet Detector";
}
