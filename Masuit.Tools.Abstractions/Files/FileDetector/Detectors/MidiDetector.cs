using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Audio)]
internal sealed class MidiDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] MidiSignatureInfo = {
        new() { Position = 0, Signature = "MThd"u8.ToArray() },
    };

    public override string Extension => "mid";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override SignatureInformation[] SignatureInformations => MidiSignatureInfo;

    public override string ToString() => "MIDI Detector";
}
