using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Audio)]
internal class WaveDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] WavSignatureInfo = {
        new() { Position = 0, Signature = new byte [] { 0x52, 0x49, 0x46, 0x46 } },
        new() { Position = 8, Signature = new byte [] { 0x57, 0x41, 0x56, 0x45 }, Presignature = new byte [] { 0x52, 0x49, 0x46, 0x46 } },
    };

    public override string Extension => "wav";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => WavSignatureInfo;

    public override string ToString() => "Wave(WAV) Detector";
}
