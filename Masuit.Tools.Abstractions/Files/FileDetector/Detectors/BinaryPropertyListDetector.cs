using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Document)]
internal class BinaryPropertyListDetector : AbstractSignatureDetector
{
    private static SignatureInformation[] BPLIST_SignatureInfo = new[]
    {
        new SignatureInformation () { Position = 0, Signature = new byte [] { 0x62, 0x70, 0x6C, 0x69, 0x73, 0x74 } },
    };

    public override string Extension => "bplist";

    protected override SignatureInformation[] SignatureInformations => BPLIST_SignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Apple Binary Property List Detector";
}
