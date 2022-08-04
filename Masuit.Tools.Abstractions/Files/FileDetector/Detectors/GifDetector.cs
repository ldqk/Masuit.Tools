using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Video)]
[FormatCategory(FormatCategory.Image)]
internal class GifDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] GifSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 } },
        new() { Position = 0, Signature = new byte [] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 } },
    };

    public override string Extension => "gif";

    protected override SignatureInformation[] SignatureInformations => GifSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Graphics Interchange Format Detector";
}
