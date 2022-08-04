using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Image)]
internal class IconDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] IcoSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x00, 0x00, 0x01, 0x00 } },
    };

    public override string Extension => "ico";

    protected override SignatureInformation[] SignatureInformations => IcoSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Icon Detector";
}
