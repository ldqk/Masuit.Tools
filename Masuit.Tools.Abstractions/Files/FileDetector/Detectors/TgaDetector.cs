using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Media;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Image)]
internal sealed class TgaDetector : IDetector
{
    public string Extension => "tga";

    public string Precondition => null;

    public string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public bool Detect(Stream stream) => stream.GetImageType() == ImageFormat.Tga;

    public override string ToString() => "Targa Detector (Experimental)";
}
