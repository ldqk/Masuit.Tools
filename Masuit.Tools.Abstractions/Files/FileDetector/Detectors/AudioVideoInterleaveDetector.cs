using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Video)]
[FormatCategory(FormatCategory.Audio)]
internal sealed class AudioVideoInterleaveDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] AviSignatureInfo = {
        new () { Position = 0, Signature = "RIFF"u8.ToArray() },
        new () { Position = 8, Signature = "AVI "u8.ToArray(), Presignature = "RIFF"u8.ToArray() },
    };

    public override string Extension => "avi";

    protected override SignatureInformation[] SignatureInformations => AviSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Audio Video Interleave(AVI) Detector";
}
