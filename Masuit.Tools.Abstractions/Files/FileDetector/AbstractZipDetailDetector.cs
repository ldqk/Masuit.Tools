using System.IO.Compression;
using System.Reflection;
using Masuit.Tools.Mime;

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
            var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
            return Files.Select(filename => archive.Entries.Any(entry => entry.FullName == filename && IsValid(filename, entry))).All(succeed => succeed);
        }
        catch
        {
            return false;
        }
    }
}
