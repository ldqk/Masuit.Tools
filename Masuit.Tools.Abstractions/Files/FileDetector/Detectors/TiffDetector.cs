using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Image)]
internal class TiffDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] TiffSignatureInfo = {
        new () { Position = 0, Signature = new byte [] { 0x49, 0x49, 0x49 } },
        new () { Position = 0, Signature = new byte [] { 0x49, 0x49, 0x2A, 0x00 } },
        new () { Position = 0, Signature = new byte [] { 0x4D, 0x4D, 0x00, 0x2A } },
        new () { Position = 0, Signature = new byte [] { 0x4D, 0x4D, 0x00, 0x2B } },
    };

    public override string Extension => "tif";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => TiffSignatureInfo;

    public override string ToString() => "Tagged Image File Format Detector";
}
