using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Document)]
internal sealed class RichTextFormatDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] RtfSignatureInfo = {
        new () { Position = 0, Signature = "{\\rtf1"u8.ToArray() },
    };

    public override string Extension => "rtf";

    protected override SignatureInformation[] SignatureInformations => RtfSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Rich Text Format Detector";
}
