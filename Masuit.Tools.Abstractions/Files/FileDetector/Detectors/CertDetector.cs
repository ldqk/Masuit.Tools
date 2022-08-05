using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Document)]
internal class CertDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] CrtSignatureInfo = {
        new() { Position = 0, Signature = Encoding.GetEncoding ( "ascii" ).GetBytes ( "-----BEGIN CERTIFICATE-----" ) },
    };

    public override string Precondition => "txt";

    public override string Extension => "crt";

    protected override SignatureInformation[] SignatureInformations => CrtSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Certificate Detector";
}
