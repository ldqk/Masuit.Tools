using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Executable)]
internal class BashShellScriptDetector : AbstractRegexSignatureDetector
{
    public override string Precondition => "txt";

    public override string Extension => "sh";

    protected override Regex Signature => new("^#!\\/(.+)\n");

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    public override string ToString() => "Bash Shell Script Detector";
}
