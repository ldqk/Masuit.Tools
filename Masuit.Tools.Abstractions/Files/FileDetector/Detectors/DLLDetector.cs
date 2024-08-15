using System.Reflection;
using System.Text;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.System)]
[FormatCategory(FormatCategory.Executable)]
internal sealed class DLLDetector : AbstractSignatureDetector
{
    public override string Precondition => "exe";

    protected override SignatureInformation[] SignatureInformations { get; } =
    {
        new()
        {
            Position = 0,
            Signature = "MZ"u8.ToArray()
        },
    };

    public override string Extension => "dll";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override bool Detect(Stream stream)
    {
        if (stream.Length < 100)
        {
            return false;
        }

        if (base.Detect(stream))
        {
            stream.Position = 60;
            var reader = new BinaryReader(stream, Encoding.UTF8, true);
            var num = reader.ReadInt32();
            if (num < 0)
            {
                return false;
            }

            stream.Position = num + 4 + 18;
            short characteristics = reader.ReadInt16();
            return (characteristics & 0x2000) != 0;
        }

        return false;
    }

    public override string ToString() => "Windows Dynamic Linkage Library Detector";
}