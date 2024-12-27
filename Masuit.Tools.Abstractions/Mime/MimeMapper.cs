using System.Collections.Generic;
using System.Text.RegularExpressions;
using Masuit.Tools.Systems;

namespace Masuit.Tools.Mime;

/// <summary>
/// 默认MIME映射器，可以根据文件扩展名获取标准内容类型。
/// </summary>
public class MimeMapper : IMimeMapper
{
    /// <summary>
    /// 默认Mime  - 如果没有找到任何其他映射则作为默认的Mime-Type
    /// </summary>
    public const string DefaultMime = "application/octet-stream";

    /// <summary>
    /// Mime类型与扩展名的映射字典(扩展名:mimetype)
    /// </summary>
    public static Dictionary<string, string> MimeTypes { get; } = new();

    /// <summary>
    /// mime类型与扩展名的映射关系(mimetype:扩展名)
    /// </summary>
    public static LookupX<string, string> ExtTypes { get; }

    static MimeMapper()
    {
        ExtTypes = DefaultMimeItems.Items.ToLookupX(x => x.MimeType, x => "." + x.Extension);
        MimeTypes = DefaultMimeItems.Items.ToDictionary(x => "." + x.Extension, x => x.MimeType);
    }

    /// <summary>
    ///
    /// </summary>
    public MimeMapper() : this(null)
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="extensions"></param>
    public MimeMapper(params MimeMappingItem[] extensions)
    {
        Extend(extensions);
    }

    /// <summary>
    /// 扩展mime映射规则的标准列表。扩展的具有更高的优先级 - 如果扩展具有与标准项相同的扩展名，则会覆盖默认的mime
    /// </summary>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public IMimeMapper Extend(params MimeMappingItem[] extensions)
    {
        if (extensions != null)
        {
            foreach (var mapping in extensions)
            {
                MimeTypes[mapping.Extension] = mapping.MimeType;
                ExtTypes[mapping.MimeType].Add(mapping.Extension);
            }
        }
        return this;
    }

    /// <summary>
    /// 返回特定文件扩展名的Content-Type，如果未找到任何对应关系，则返回默认值
    /// </summary>
    /// <param name="fileExtension"></param>
    /// <returns></returns>
    public string GetMimeFromExtension(string fileExtension)
    {
        fileExtension = (fileExtension ?? string.Empty).ToLower();
        return MimeTypes.TryGetValue(fileExtension, out var type) ? type : DefaultMime;
    }

    /// <summary>
    /// 返回特定Content-Type的文件扩展名，如果未找到任何对应关系，则返回空值
    /// </summary>
    /// <param name="mime"></param>
    /// <returns></returns>
    public List<string> GetExtensionFromMime(string mime)
    {
        mime = (mime ?? string.Empty).ToLower();
        if (ExtTypes.Contains(mime))
        {
            return ExtTypes[mime];
        }
        return [];
    }

    /// <summary>
    /// 根据路径获取MimeType
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string GetMimeFromPath(string path)
    {
        var extension = Path.GetExtension(path);
        return GetMimeFromExtension(extension);
    }
}