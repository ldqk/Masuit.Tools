using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Document)]
internal class XMLDetector : AbstractRegexSignatureDetector
{
    public override string Precondition => "txt";

    public override string Extension => "xml";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override Regex Signature => new("^<\\?xml[ \t\n\r]+version=\"[0-9]+\\.[0-9]+\"[ \t\n\r]+([a-zA-Z0-9]+=\"[a-zA-Z0-9\\-_]+\"[ \t\n\r]*)*[ \t\n\r]+\\?>");

    public override string ToString() => "eXtensible Markup Language Document Detector";
}
