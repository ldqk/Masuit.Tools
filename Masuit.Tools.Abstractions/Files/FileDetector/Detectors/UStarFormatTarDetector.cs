using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Archive)]
internal class UStarFormatTarDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] TarSignatureInfo = {
        new () { Position = 0x101, Signature = new byte [] { 0x75, 0x73, 0x74, 0x61, 0x72, 0x00, 0x30, 0x30 } },
        new () { Position = 0x101, Signature = new byte [] { 0x75, 0x73, 0x74, 0x61, 0x72, 0x20, 0x20, 0x00 } },
    };

    public override string Extension => "tar";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => TarSignatureInfo;

    public override string ToString() => "UStar(TAR) Detector";
}
