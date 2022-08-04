using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Compression)]
internal class XarDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] XarSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x78, 0x61, 0x72, 0x21 } },
    };

    public override string Extension => "xar";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => XarSignatureInfo;

    public override string ToString() => "XAR Detector";
}
