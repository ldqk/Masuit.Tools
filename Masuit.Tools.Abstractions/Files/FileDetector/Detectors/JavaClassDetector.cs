using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Executable)]
internal class JavaClassDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] ClassSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0xCA, 0xFE, 0xBA, 0xBE } },
    };

    public override string Extension => "class";

    protected override SignatureInformation[] SignatureInformations => ClassSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Java Bytecode Detector";
}
