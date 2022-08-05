using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Video)]
[FormatCategory(FormatCategory.Audio)]
internal class AudioVideoInterleaveDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] AviSignatureInfo = {
        new () { Position = 0, Signature = new byte [] { 0x52, 0x49, 0x46, 0x46 } },
        new () { Position = 8, Signature = new byte [] { 0x41, 0x56, 0x49, 0x20 }, Presignature = new byte [] { 0x52, 0x49, 0x46, 0x46 } },
    };

    public override string Extension => "avi";

    protected override SignatureInformation[] SignatureInformations => AviSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Audio Video Interleave(AVI) Detector";
}
