using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Image)]
internal class BitmapDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] BmpSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x42, 0x4D } },
        new() { Position = 6, Signature = new byte [] { 0x00, 0x00, 0x00, 0x00 }, Presignature = new byte [] { 0x42, 0x4D } },
    };

    public override string Extension => "bmp";

    protected override SignatureInformation[] SignatureInformations => BmpSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Bitmap(BMP) Detector";
}
