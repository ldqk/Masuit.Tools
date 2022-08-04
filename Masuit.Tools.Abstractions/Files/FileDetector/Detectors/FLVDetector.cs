using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Video)]
internal class FLVDetector : AbstractSignatureDetector
{
    private static SignatureInformation[] FLV_SignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x46, 0x4C, 0x56 } },
    };

    public override string Extension => "flv";

    protected override SignatureInformation[] SignatureInformations => FLV_SignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Flash Video Detector";
}
