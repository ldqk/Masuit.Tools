using System;
using System.Collections.Generic;

namespace Masuit.Tools.Systems;

public static class IDictionaryExtensions
{
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (dictionary.IsReadOnly || dictionary.ContainsKey(key))
            return false;
        dictionary.Add(key, value);
        return true;
    }
}