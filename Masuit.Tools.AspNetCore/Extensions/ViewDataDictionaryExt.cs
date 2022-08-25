using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;

namespace Masuit.Tools.AspNetCore.Extensions;

public static class ViewDataDictionaryExt
{
    public static T GetValue<T>(this ViewDataDictionary dic, string s) where T : class
    {
        return dic[s] as T;
    }

    public static T GetValueOrDefault<T>(this ViewDataDictionary dic, string s, T defaultValue)
    {
        return (T)(dic[s] ?? defaultValue);
    }

    public static T GetValueOrDefault<T>(this ViewDataDictionary dic, string s, Func<T> defaultValue)
    {
        return (T)(dic[s] ?? defaultValue());
    }
}