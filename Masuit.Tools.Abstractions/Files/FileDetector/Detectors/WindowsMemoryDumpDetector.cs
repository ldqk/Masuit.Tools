using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.System)]
internal class WindowsMemoryDumpDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] DmpSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x4D, 0x44, 0x4D, 0x50, 0x93, 0xA7 } },
        new() { Position = 0, Signature = new byte [] { 0x50, 0x41, 0x47, 0x45, 0x44, 0x55 } },
    };

    public override string Extension => "dmp";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => DmpSignatureInfo;

    public override string ToString() => "Windows Memory Dump File Detector";
}
