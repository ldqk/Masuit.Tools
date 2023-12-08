using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Audio)]
internal sealed class WaveDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] WavSignatureInfo = {
        new() { Position = 0, Signature = "RIFF"u8.ToArray() },
        new() { Position = 8, Signature = "WAVE"u8.ToArray(), Presignature = "RIFF"u8.ToArray() },
    };

    public override string Extension => "wav";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => WavSignatureInfo;

    public override string ToString() => "Wave(WAV) Detector";
}
