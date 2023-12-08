using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Image)]
[FormatCategory(FormatCategory.Video)]
internal sealed class WebPDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] WebpSignatureInfo = {
        new() { Position = 0, Signature = "RIFF"u8.ToArray() },
        new() { Position = 8, Signature = "WEBP"u8.ToArray(), Presignature = "RIFF"u8.ToArray() },
    };

    public override string Extension => "webp";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => WebpSignatureInfo;

    public override string ToString() => "WebP Detector";
}
