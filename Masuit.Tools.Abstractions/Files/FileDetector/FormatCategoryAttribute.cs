using System;
using System.ComponentModel;

namespace Masuit.Tools.Files.FileDetector;

[Flags]
public enum FormatCategory : uint
{
    [Description("图片")]
    Image = 1,

    [Description("视频")]
    Video = 2,

    [Description("音频")]
    Audio = 4,

    [Description("档案包")]
    Archive = 8,

    [Description("压缩包")]
    Compression = 16,

    [Description("文档")]
    Document = 32,

    [Description("系统")]
    System = 64,

    [Description("可执行")]
    Executable = 128,

    [Description("其他二进制")]
    All = 0xffffffff
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FormatCategoryAttribute : Attribute
{
    public FormatCategory Category { get; }

    public FormatCategoryAttribute(FormatCategory category)
    {
        Category = category;
    }
}
