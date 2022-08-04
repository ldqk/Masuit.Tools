using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Audio)]
internal class FlacDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] FlacSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x66, 0x4C, 0x61, 0x43 } },
    };

    public override string Extension => "flac";

    protected override SignatureInformation[] SignatureInformations => FlacSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "FLAC Detector";
}
