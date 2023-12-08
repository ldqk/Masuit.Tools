using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector.Detectors;

[FormatCategory(FormatCategory.Archive)]
internal sealed class MicrosoftInstallerDetector : AbstractCompoundFileDetailDetector
{
    public override IEnumerable<string> Chunks
    {
        get
        {
            yield return "SummaryInformation";
        }
    }

    public override string Extension => "msi";

    public override string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public override List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected override bool IsValidChunk(string chunkName, byte[] chunkData)
    {
        return chunkName == "SummaryInformation" && Encoding.ASCII.GetString(chunkData, 0, chunkData.Length).IndexOf("Installation Database", StringComparison.Ordinal) != -1;
    }

    public override string ToString() => "Microsoft Installer Setup File Detector";
}
