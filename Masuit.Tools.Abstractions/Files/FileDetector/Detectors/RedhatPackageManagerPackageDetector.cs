using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Archive)]
internal sealed class RedhatPackageManagerPackageDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] RpmSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0xED, 0xAB, 0xEE, 0xDB } },
    };

    public override string Extension => "rpm";

    protected override SignatureInformation[] SignatureInformations => RpmSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "RedHat Package Manager Package File Detector";
}
