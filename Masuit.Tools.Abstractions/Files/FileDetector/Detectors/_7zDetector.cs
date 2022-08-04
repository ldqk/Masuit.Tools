using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Archive)]
[FormatCategory(FormatCategory.Compression)]
internal class _7zDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] _7ZSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C } },
    };

    public override string Extension => "7z";

    protected override SignatureInformation[] SignatureInformations => _7ZSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "7Z Detector";
}
