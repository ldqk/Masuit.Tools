using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Masuit.Tools.Reflection;

namespace Masuit.Tools.Files;

/// <summary>
/// INI文件操作辅助类
/// </summary>
public class IniFile
{
    private readonly Dictionary<string, IniSection> _sections;
    private readonly StringComparer _stringComparer;
    private readonly BoolOptions _boolOptions;
    private readonly string _path;

    /// <summary>
    /// 全局配置节
    /// </summary>
    public const string DefaultSectionName = "General";

    /// <summary>
    ///
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="comparer"></param>
    /// <param name="boolOptions"></param>
    public IniFile(string path, StringComparer comparer = null, BoolOptions boolOptions = null)
    {
        _path = path;
        _stringComparer = comparer ?? StringComparer.CurrentCultureIgnoreCase;
        _sections = new Dictionary<string, IniSection>(_stringComparer ?? StringComparer.CurrentCultureIgnoreCase);
        _boolOptions = boolOptions ?? new BoolOptions();
        if (File.Exists(path))
        {
            using StreamReader reader = new(path, Encoding.UTF8);
            IniSection section = null;
            while (reader.ReadLine() is { } line)
            {
                ParseLine(line, ref section);
            }
        }
    }

    /// <summary>
    /// 重新加载文件
    /// </summary>
    public void Reload()
    {
        _sections.Clear();
        using StreamReader reader = new(_path, Encoding.UTF8);
        IniSection section = null;
        while (reader.ReadLine() is { } line)
        {
            ParseLine(line, ref section);
        }
    }

    private void ParseLine(string line, ref IniSection section)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        line = line.Trim();
        if (line[0] == ';')
        {
            return;
        }

        if (line[0] == '[')
        {
            var name = line.Trim('[', ']').Trim();
            if (name.Length > 0 && !_sections.TryGetValue(name, out section))
            {
                section = new IniSection(name, _stringComparer);
                _sections.Add(section.Name, section);
            }
        }
        else
        {
            string name, value;
            var strs = line.Split(';')[0].Split('=');
            if (strs.Length == 1)
            {
                name = line.Trim();
                value = string.Empty;
            }
            else
            {
                name = strs[0].Trim();
                value = strs[1].Trim();
            }

            if (name.Length <= 0)
            {
                return;
            }

            if (section == null)
            {
                section = new IniSection(DefaultSectionName, _stringComparer);
                _sections.Add(section.Name, section);
            }

            if (section.TryGetValue(name, out var item))
            {
                item.Value = value;
            }
            else
            {
                item = new IniItem
                {
                    Name = name,
                    Value = value
                };
                section.Add(name, item);
            }
        }
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    public void Save()
    {
        using StreamWriter writer = new(_path, false, Encoding.UTF8);
        bool firstLine = true;
        foreach (var section in _sections.Values.Where(section => section.Count > 0))
        {
            if (firstLine)
            {
                firstLine = false;
            }
            else
            {
                writer.WriteLine();
            }

            writer.WriteLine($"[{section.Name}]");
            foreach (var setting in section.Values)
            {
                writer.WriteLine(setting.ToString());
            }
        }
    }

    /// <summary>
    /// 异步保存配置文件
    /// </summary>
    /// <returns></returns>
    public async Task SaveAsync()
    {
        using StreamWriter writer = new(_path, false, Encoding.UTF8);
        var firstLine = true;
        foreach (var section in _sections.Values.Where(section => section.Count > 0))
        {
            if (firstLine)
            {
                firstLine = false;
            }
            else
            {
                await writer.WriteLineAsync();
            }

            await writer.WriteLineAsync($"[{section.Name}]");
            foreach (var setting in section.Values)
            {
                await writer.WriteLineAsync(setting.ToString());
            }
        }
    }

    #region Read values

    /// <summary>
    /// 获取指定节的指定键的值
    /// </summary>
    /// <param name="section">节</param>
    /// <param name="key">键</param>
    /// <param name="defaultValue">获取不到时的默认值</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string GetValue(string section, string key, string defaultValue = null)
    {
        if (section == null)
        {
            throw new ArgumentNullException(nameof(section));
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (!_sections.TryGetValue(section, out var iniSection))
        {
            return defaultValue;
        }

        if (iniSection.TryGetValue(key, out var iniSetting))
        {
            return iniSetting.Value;
        }

        return defaultValue;
    }

    /// <summary>
    /// 获取指定节的指定键的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="section">配置节</param>
    /// <param name="key">键</param>
    /// <param name="defaultValue">获取不到时的默认值</param>
    /// <returns></returns>
    public T GetValue<T>(string section, string key, T defaultValue = default) where T : IConvertible
    {
        return GetValue(section, key).TryConvertTo(defaultValue);
    }

    /// <summary>
    /// 所有的配置节
    /// </summary>
    /// <returns></returns>
    public List<IniSection> GetSections() => _sections.Values.ToList();

    /// <summary>
    /// 获取指定节的所有键值对
    /// </summary>
    /// <param name="section">节</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Dictionary<string, string> GetSection(string section = DefaultSectionName)
    {
        if (section == null)
        {
            throw new ArgumentNullException(nameof(section));
        }

        var values = _sections.TryGetValue(section, out var iniSection) ? iniSection.Values : Enumerable.Empty<IniItem>();
        return values.ToDictionary(x => x.Name, x => x.Value);
    }

    /// <summary>
    /// 获取指定节的配置并绑定到指定类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="section">节</param>
    /// <returns></returns>
    public T GetSection<T>(string section = DefaultSectionName) where T : class, new()
    {
        var dic = GetSection(section);
        var obj = new T();
        var properties = typeof(T).GetProperties().ToDictionary(p => p.GetAttribute<IniPropertyAttribute>()?.Name ?? p.Name);
        foreach (var item in dic.Where(item => properties.ContainsKey(item.Key)))
        {
            properties[item.Key].SetValue(obj, item.Value.ConvertTo(properties[item.Key].PropertyType));
        }

        return obj;
    }

    #endregion Read values

    #region Write values

    /// <summary>
    /// 设置指定节的指定键的值
    /// </summary>
    /// <param name="section">节</param>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void SetValue(string section, string key, string value)
    {
        if (section == null)
        {
            throw new ArgumentNullException(nameof(section));
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (!_sections.TryGetValue(section, out var sec))
        {
            sec = new IniSection(section, _stringComparer);
            _sections.Add(sec.Name, sec);
        }

        if (!sec.TryGetValue(key, out var item))
        {
            item = new IniItem
            {
                Name = key
            };
            sec.Add(key, item);
        }

        item.Value = value ?? string.Empty;
    }

    /// <summary>
    /// 设置指定节的指定键的值
    /// </summary>
    /// <param name="section">节</param>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    public void SetValue(string section, string key, bool value) => SetValue(section, key, _boolOptions.ToString(value));

    /// <summary>
    /// 设置指定节的指定键的值
    /// </summary>
    /// <param name="section">节</param>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    public void SetValue<T>(string section, string key, T value) where T : IConvertible => SetValue(section, key, value.ToString());

    /// <summary>
    /// 清空配置节
    /// </summary>
    /// <param name="section"></param>
    public void ClearSection(string section)
    {
        if (_sections.TryGetValue(section, out var sec))
        {
            sec.Clear();
        }
    }

    /// <summary>
    /// 清空所有配置节
    /// </summary>
    public void ClearAllSection()
    {
        _sections.Clear();
    }

    #endregion Write values
}