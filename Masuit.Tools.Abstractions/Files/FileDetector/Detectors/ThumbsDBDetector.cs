using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.System)]
internal class ThumbsDBDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] ThumbdbSignatureInfo = {
        new () { Position = 0, Signature = new byte [] { 0xFD, 0xFF, 0xFF, 0xFF } },
    };

    public override string Extension => "db";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => ThumbdbSignatureInfo;

    public override string ToString() => "Thumbs.db Detector";
}
