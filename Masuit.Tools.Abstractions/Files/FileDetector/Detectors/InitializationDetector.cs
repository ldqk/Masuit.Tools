using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Document)]
internal class InitializationDetector : AbstractRegexSignatureDetector
{
    public override string Precondition => "txt";

    public override string Extension => "ini";

    protected override Regex Signature => new("^(\\[(.*)\\]\r?\n((((.*)=(.*))*|(;(.*)))\r?\n)*)+");

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Initialization File Detector";
}
