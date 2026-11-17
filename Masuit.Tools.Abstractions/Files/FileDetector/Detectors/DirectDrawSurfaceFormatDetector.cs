using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Image)]
internal sealed class DirectDrawSurfaceFormatDetector : AbstractSignatureDetector
{
    private static readonly SignatureInformation[] DdsSignatureInfo = {
        new() { Position = 0, Signature = "DDS "u8.ToArray() },
    };

    public override string Extension => "dds";

    protected override SignatureInformation[] SignatureInformations => DdsSignatureInfo;

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override bool Detect(Stream stream)
    {
        if (base.Detect(stream))
        {
            byte[] buffer = new byte[4];

            if (!TryReadExactly(stream, buffer))
            {
                return false;
            }

            int dwSize = (buffer[3] << 24) + (buffer[2] << 16) + (buffer[1] << 8) + buffer[0];
            if (dwSize != 0x7C)
            {
                return false;
            }

            if (!TryReadExactly(stream, buffer))
            {
                return false;
            }

            int dwFlags = (buffer[3] << 24) + (buffer[2] << 16) + (buffer[1] << 8) + buffer[0];

            if ((dwFlags & 0x1) != 0 && (dwFlags & 0x2) != 0 && (dwFlags & 0x4) != 0 && (dwFlags & 0x1000) != 0)
            {
                return true;
            }
        }
        return false;
    }

    private static bool TryReadExactly(Stream stream, byte[] buffer)
    {
        int bytesRead = 0;
        while (bytesRead < buffer.Length)
        {
            int read = stream.Read(buffer, bytesRead, buffer.Length - bytesRead);
            if (read == 0)
            {
                return false;
            }

            bytesRead += read;
        }

        return true;
    }

    public override string ToString() => "DirectDraw Surface(DDS) Detector";
}
