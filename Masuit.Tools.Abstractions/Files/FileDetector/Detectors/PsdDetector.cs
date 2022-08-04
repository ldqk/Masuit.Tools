using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Image)]
internal class PsdDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] PsdSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x38, 0x42, 0x50, 0x53 } },
    };

    public override string Extension => "psd";

    protected override SignatureInformation[] SignatureInformations => PsdSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Photoshop File(PSD) Detector";
}
