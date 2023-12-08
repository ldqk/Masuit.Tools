using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Audio)]
internal sealed class FlacDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] FlacSignatureInfo = {
        new() { Position = 0, Signature = "fLaC"u8.ToArray() },
    };

    public override string Extension => "flac";

    protected override SignatureInformation[] SignatureInformations => FlacSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "FLAC Detector";
}
