using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Masuit.Tools.Systems;

public class LookupX<TKey, TElement> : IEnumerable<List<TElement>>
{
    private readonly IDictionary<TKey, List<TElement>> _dictionary;

    public LookupX(Dictionary<TKey, List<TElement>> dic)
    {
        _dictionary = dic;
    }

    public LookupX(ConcurrentDictionary<TKey, List<TElement>> dic)
    {
        _dictionary = dic;
    }

    public IEnumerator<List<TElement>> GetEnumerator()
    {
        return _dictionary.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Contains(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    public int Count => _dictionary.Count;

    public List<TElement> this[TKey key] => _dictionary.TryGetValue(key, out var value) ? value : new List<TElement>();
}