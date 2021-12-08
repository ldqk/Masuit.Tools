using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Masuit.Tools.AspNetCore.Mime
{
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
        /// 在文件路径中搜索文件扩展名的默认正则表达式
        /// </summary>
        private readonly Regex _pathExtensionPattern = new Regex("\\.(\\w*)$");

        /// <summary>
        /// Mime类型与扩展名的映射字典(扩展名:mimetype)
        /// </summary>
        public static Dictionary<string, string> MimeTypes { get; } = new Dictionary<string, string>();

        /// <summary>
        /// mime类型与扩展名的映射关系(mimetype:扩展名)
        /// </summary>
        public static Dictionary<string, string> ExtTypes { get; } = new Dictionary<string, string>();

        static MimeMapper()
        {
            foreach (var item in DefaultMimeItems.Items)
            {
                MimeTypes["." + item.Extension] = item.MimeType;
                ExtTypes[item.MimeType] = "." + item.Extension;
            }
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
                    ExtTypes[mapping.MimeType] = mapping.Extension;
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
            return MimeTypes.ContainsKey(fileExtension) ? MimeTypes[fileExtension] : DefaultMime;
        }

        /// <summary>
        /// 返回特定Content-Type的文件扩展名，如果未找到任何对应关系，则返回空值
        /// </summary>
        /// <param name="mime"></param>
        /// <returns></returns>
        public string GetExtensionFromMime(string mime)
        {
            mime = (mime ?? string.Empty).ToLower();
            return ExtTypes.ContainsKey(mime) ? ExtTypes[mime] : "";
        }

        /// <summary>
        /// 根据路径获取MimeType
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetMimeFromPath(string path)
        {
            var extension = GetExtension(path);
            return GetMimeFromExtension(extension);
        }

        /// <summary>
        /// 获取扩展名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected string GetExtension(string path)
        {
            var match = _pathExtensionPattern.Match(path ?? string.Empty);
            if (match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }

            return null;
        }
    }
}