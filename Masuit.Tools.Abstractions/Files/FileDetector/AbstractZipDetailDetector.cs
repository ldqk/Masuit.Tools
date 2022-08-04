using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector;

public abstract class AbstractZipDetailDetector : IDetector
{
    public abstract IEnumerable<string> Files { get; }

    public abstract string Extension { get; }

    public virtual string Precondition => null;

    public virtual string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public virtual List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    protected virtual bool IsValid(string filename, ZipArchiveEntry entry)
    {
        return true;
    }

    public bool Detect(Stream stream)
    {
        try
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
            foreach (string filename in Files)
            {
                bool succeed = archive.Entries.Any(entry => entry.FullName == filename && IsValid(filename, entry));
                if (!succeed)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
