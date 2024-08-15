using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Archive)]
internal sealed class DMGDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] DmgSignatureInfo = {
        new() { Position = 510, Signature = new byte [] { 0x55, 0xAA, 0x45, 0x46, 0x49, 0x20, 0x50,0x41,0x52,0x54 } },
    };

    public override string Extension => "dmg";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => DmgSignatureInfo;

    public override string ToString() => "Apple Disk Mount Image(DMG) Detector";
}
