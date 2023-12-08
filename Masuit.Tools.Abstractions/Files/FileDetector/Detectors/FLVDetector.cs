using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Video)]
internal sealed class FLVDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] FlvSignatureInfo = {
        new() { Position = 0, Signature = "FLV"u8.ToArray() },
    };

    public override string Extension => "flv";

    protected override SignatureInformation[] SignatureInformations => FlvSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Flash Video Detector";
}
