using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Executable)]
internal sealed class EXEDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] ExeSignatureInfo = {
        new() { Position = 0, Signature = "MZ"u8.ToArray() },
    };

    public override string Extension => "exe";

    protected override SignatureInformation[] SignatureInformations => ExeSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override bool Detect(Stream stream)
    {
        if (stream.Length<100)
        {
            return false;
        }
        if (base.Detect(stream))
        {
            stream.Position = 60;
            var reader = new BinaryReader(stream, Encoding.UTF8, true);
            stream.Position = reader.ReadInt32();
            return reader.ReadByte() == 0x50 && reader.ReadByte() == 0x45 && reader.ReadByte() == 0 && reader.ReadByte() == 0;
        }
        return false;
    }

    public override string ToString() => "Portable Execution File Format Detector";
}
