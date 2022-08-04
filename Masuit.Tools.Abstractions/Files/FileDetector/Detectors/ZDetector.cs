using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Compression)]
internal class ZDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] ZSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x1F, 0x9D } },
        new() { Position = 0, Signature = new byte [] { 0x1F, 0xA0 } },
    };

    public override string Extension => "z";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => ZSignatureInfo;

    public override string ToString() => "Z Detector";
}
