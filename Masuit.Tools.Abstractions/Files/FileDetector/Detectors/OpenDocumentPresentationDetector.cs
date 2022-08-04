using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Document)]
internal class OpenDocumentPresentationDetector : AbstractZipDetailDetector
{
    public override IEnumerable<string> Files
    {
        get
        {
            yield return "mimetype";
        }
    }

    public override string Precondition => "zip";

    public override string Extension => "odp";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override bool IsValid(string filename, ZipArchiveEntry entry)
    {
        if (filename == "mimetype")
        {
            using Stream mimetypeStream = entry.Open();
            byte[] buffer = new byte["application/vnd.oasis.opendocument.presentation".Length];
            if (mimetypeStream.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                return false;
            }

            if (Encoding.GetEncoding("ascii").GetString(buffer, 0, buffer.Length) != "application/vnd.oasis.opendocument.presentation")
            {
                return false;
            }
        }

        return base.IsValid(filename, entry);
    }

    public override string ToString() => "OpenDocument Presentation File Detector";
}
