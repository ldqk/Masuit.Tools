using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Systems;

public class LookupX<TKey, TElement> : IEnumerable<Grouping<TKey, TElement>>
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

    public IEnumerator<Grouping<TKey, TElement>> GetEnumerator()
    {
        return _dictionary.Select(pair => new Grouping<TKey, TElement>(pair.Key, pair.Value)).GetEnumerator();
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

public class Grouping<TKey, TElement> : IEnumerable<TElement>
{
    private readonly List<TElement> _list;

    internal Grouping(TKey key, List<TElement> list)
    {
        Key = key;
        _list = list;
    }

    public IEnumerator<TElement> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TKey Key { get; }

    public void Deconstruct(out TKey key, out IEnumerable<TElement> elements)
    {
        key = Key;
        elements = _list;
    }
}
