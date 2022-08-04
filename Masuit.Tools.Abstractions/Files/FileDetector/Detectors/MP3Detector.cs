using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Audio)]
internal class MP3Detector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] Mp3SignatureInfo = {
        new () { Position = 0, Signature = new byte [] { 0xFF, 0xFB } },
        new () { Position = 0, Signature = new byte [] { 0x49, 0x44, 0x33 } },
    };

    public override string Extension => "mp3";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => Mp3SignatureInfo;

    public override string ToString() => "MP3 Detector";
}
