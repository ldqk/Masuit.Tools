using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Archive)]
internal class DMGDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] DmgSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x78, 0x01, 0x73, 0x0D, 0x62, 0x62, 0x60 } },
    };

    public override string Extension => "dmg";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => DmgSignatureInfo;

    public override string ToString() => "Apple Disk Mount Image(DMG) Detector";
}
