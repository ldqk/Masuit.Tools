using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Archive)]
internal class MikeOBrienPackDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] MpqSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x4D, 0x50, 0x51, 0x1A } },
    };

    public override string Extension => "mpq";

    protected override SignatureInformation[] SignatureInformations => MpqSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Mo'PaQ Detector";
}
