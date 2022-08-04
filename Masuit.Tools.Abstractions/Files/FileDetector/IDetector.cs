using System.Collections.Generic;
using System.IO;

namespace Masuit.Tools.Files.FileDetector;

public interface IDetector
{
    string Precondition { get; }

    string Extension { get; }

    bool Detect(Stream stream);

    string MimeType { get; }

    List<FormatCategory> FormatCategories { get; }
}

public class NoneDetector:IDetector
{
    public string Precondition { get; }
    public string Extension { get; }

    public bool Detect(Stream stream)
    {
        return false;
    }

    public string MimeType { get; }

    public List<FormatCategory> FormatCategories { get; }=new List<FormatCategory>();

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return "不支持的文件格式，请自己实现针对该文件的IDetector";
    }
}