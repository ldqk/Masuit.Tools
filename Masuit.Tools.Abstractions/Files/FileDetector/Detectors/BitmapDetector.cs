using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Image)]
internal sealed class BitmapDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] BmpSignatureInfo = {
        new() { Position = 0, Signature = "BM"u8.ToArray() },
        new() { Position = 6, Signature = new byte [] { 0x00, 0x00, 0x00, 0x00 }, Presignature = "BM"u8.ToArray() },
    };

    public override string Extension => "bmp";

    protected override SignatureInformation[] SignatureInformations => BmpSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Bitmap(BMP) Detector";
}
