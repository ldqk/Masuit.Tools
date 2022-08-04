using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Video)]
[FormatCategory(FormatCategory.Executable)]
internal class ShockwaveFlashDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] SwfSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x43, 0x57, 0x53 } },
        new() { Position = 0, Signature = new byte [] { 0x46, 0x57, 0x53 } },
    };

    public override string Extension => "swf";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => SwfSignatureInfo;

    public override string ToString() => "Shockwave Flash(SWF) Detector";
}
