using System.Reflection;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Document)]
internal sealed class VSDXDetector : AbstractZipDetailDetector
{
    public override IEnumerable<string> Files
    {
        get
        {
            yield return "[Content_Types].xml";
            yield return "_rels/.rels";
            yield return "visio/_rels/document.xml.rels";
        }
    }

    public override string Precondition => "zip";

    public override string Extension => "vsdx";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Microsoft Visio Open XML Document(DOCX) Detector";
}