using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Compression)]
internal class GzDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] GzSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x1F, 0x8B } },
    };

    public override string Extension => "gz";

    protected override SignatureInformation[] SignatureInformations => GzSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "GZ Detector";
}
