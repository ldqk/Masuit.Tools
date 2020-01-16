using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Mvc.Mime
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
        /// 扩展的Mime类型的默认字典(Content types)
        /// </summary>
        private static Dictionary<string, string> _items;

        public MimeMapper() : this(null)
        {
        }

        public MimeMapper(params MimeMappingItem[] extensions)
        {
            _items = new Dictionary<string, string>();
            foreach (var mapping in DefaultMimeItems.Items)
            {
                _items.Add(mapping.Extension, mapping.MimeType);
            }
            Extend(extensions);
        }

        /// <summary>
        /// 扩展mime映射规则的标准列表。扩展的具有更高的优先级 - 如果扩展具有与标准项相同的扩展名，则会覆盖默认的mime
        /// </summary>
        /// <param name="extensions"></param>
        /// <returns></returns>
        public IMimeMapper Extend(params MimeMappingItem[] extensions)
        {
            if (extensions == null)
            {
                return this;
            }

            foreach (var mapping in extensions)
            {
                if (_items.ContainsKey(mapping.Extension))
                {
                    _items[mapping.Extension] = mapping.MimeType;
                }
                else
                {
                    _items.Add(mapping.Extension, mapping.MimeType);
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
            fileExtension = fileExtension.Trim().StartsWith(".") ? fileExtension.Replace(".", "") : fileExtension;
            return _items.ContainsKey(fileExtension) ? _items[fileExtension] : DefaultMime;
        }

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetMimeFromPath(string path)
        {
            var extension = GetExtension(path);
            return GetMimeFromExtension(extension);
        }

        protected string GetExtension(string path)
        {
            var match = _pathExtensionPattern.Match(path ?? string.Empty);
            return match.Groups.Count > 1 ? match.Groups[1].Value : null;
        }
    }
}