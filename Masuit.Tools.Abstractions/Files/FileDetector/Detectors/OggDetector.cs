using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Audio)]
[FormatCategory(FormatCategory.Video)]
internal class OggDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] OggSignatureInfo = {
        new () { Position = 0, Signature = new byte [] { 0x4F, 0x67, 0x67, 0x53 } },
    };

    public override string Extension => "ogg";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => OggSignatureInfo;

    public override string ToString() => "OGG Detector";
}
