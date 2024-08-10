using System.Collections;
using System.Collections.Immutable;

namespace Masuit.Tools.TextDiff;

public class SemanticsImmutableList<T>(ImmutableList<T> list) : IImmutableList<T>, IEquatable<IImmutableList<T>>
{
	#region IImutableList implementation

	public T this[int index] => list[index];

	public int Count => list.Count;

	public IImmutableList<T> Add(T value) => list.Add(value).WithValueSemantics();

	public IImmutableList<T> AddRange(IEnumerable<T> items) => list.AddRange(items).WithValueSemantics();

	public IImmutableList<T> Clear() => list.Clear().WithValueSemantics();

	public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

	public int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer) => list.IndexOf(item, index, count, equalityComparer);

	IImmutableList<T> IImmutableList<T>.Insert(int index, T element) => list.Insert(index, element).WithValueSemantics();

	public SemanticsImmutableList<T> Insert(int index, T element) => list.Insert(index, element).WithValueSemantics();

	public IImmutableList<T> InsertRange(int index, IEnumerable<T> items) => list.InsertRange(index, items).WithValueSemantics();

	public int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer) => list.LastIndexOf(item, index, count, equalityComparer);

	public IImmutableList<T> Remove(T value, IEqualityComparer<T>? equalityComparer) => list.Remove(value, equalityComparer).WithValueSemantics();

	public IImmutableList<T> RemoveAll(Predicate<T> match) => list.RemoveAll(match).WithValueSemantics();

	IImmutableList<T> IImmutableList<T>.RemoveAt(int index) => list.RemoveAt(index).WithValueSemantics();

	public SemanticsImmutableList<T> RemoveAt(int index) => list.RemoveAt(index).WithValueSemantics();

	public IImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer) => list.RemoveRange(items, equalityComparer).WithValueSemantics();

	public IImmutableList<T> RemoveRange(int index, int count) => list.RemoveRange(index, count).WithValueSemantics();

	public IImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer) => list.Replace(oldValue, newValue, equalityComparer).WithValueSemantics();

	public IImmutableList<T> SetItem(int index, T value) => list.SetItem(index, value);

	IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

	#endregion IImutableList implementation

	public ImmutableList<T>.Builder ToBuilder() => list.ToBuilder();

	public bool IsEmpty => list.IsEmpty;

	public override bool Equals(object obj) => Equals(obj as IImmutableList<T>);

	public bool Equals(IImmutableList<T>? other) => this.SequenceEqual(other ?? ImmutableList<T>.Empty);

	public override int GetHashCode()
	{
		unchecked
		{
			return this.Aggregate(19, (h, i) => h * 19 + i?.GetHashCode() ?? 0);
		}
	}

	public static implicit operator SemanticsImmutableList<T>(ImmutableList<T> list) => list.WithValueSemantics();

	public static bool operator ==(SemanticsImmutableList<T> left, SemanticsImmutableList<T> right) => left.Equals(right);

	public static bool operator !=(SemanticsImmutableList<T> left, SemanticsImmutableList<T> right) => !left.Equals(right);
}

internal static class Ex
{
	public static SemanticsImmutableList<T> WithValueSemantics<T>(this ImmutableList<T> list) => new(list);
}