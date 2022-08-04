using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.System)]
internal class WindowsShortcutDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] LnkSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x4C, 0x00, 0x00, 0x00, 0x01, 0x14, 0x02, 0x00 } },
    };

    public override string Extension => "lnk";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => LnkSignatureInfo;

    public override string ToString() => "Windows Shortcut File Detector";
}
