using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Archive)]
internal sealed class ISODetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] IsoSignatureInfo = {
        new() { Position = 0, Signature = "CD001"u8.ToArray() },
    };

    public override string Extension => "iso";

    protected override SignatureInformation[] SignatureInformations => IsoSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "ISO-9660 Disc Image Detector";
}
