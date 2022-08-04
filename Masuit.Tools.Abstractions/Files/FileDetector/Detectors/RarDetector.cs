using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Archive)]
[FormatCategory(FormatCategory.Compression)]
internal class RarDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] RarSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 } },
        new() { Position = 0, Signature = new byte [] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 } },
    };

    public override string Extension => "rar";

    protected override SignatureInformation[] SignatureInformations => RarSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "RAR Detector";
}
