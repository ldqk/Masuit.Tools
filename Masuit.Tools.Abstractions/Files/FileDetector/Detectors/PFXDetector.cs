using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Document)]
internal class PFXDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] PfxSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x30, 0x82, 0x06 } },
    };

    public override string Extension => "pfx";

    protected override SignatureInformation[] SignatureInformations => PfxSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Microsoft Personal inFormation eXchange Certificate Detector";
}
