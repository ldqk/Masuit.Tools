using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Compression)]
internal sealed class LzhDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] LzhSignatureInfo = {
        new() { Position = 2, Signature = "-lh"u8.ToArray() },
    };

    public override string Extension => "lzh";

    protected override SignatureInformation[] SignatureInformations => LzhSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "LZH Detector";
}
