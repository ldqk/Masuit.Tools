using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Archive)]
internal class QuakeArchiveDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] PakSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x50, 0x41, 0x43, 0x4B } },
    };

    public override string Extension => "pak";

    protected override SignatureInformation[] SignatureInformations => PakSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Quake™ Package Detector";
}
