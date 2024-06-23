using System;

namespace Masuit.Tools.Mvc.Internal
{
    public record struct ByteRange : IEquatable<ByteRange>
    {
        public long Start { get; set; }
        public long End { get; set; }

        /// <summary>指示当前对象是否等于同一类型的另一个对象。</summary>
        /// <param name="other">一个与此对象进行比较的对象。</param>
        /// <returns>如果当前对象等于 <paramref name="other" /> 参数，则为 true；否则为 false。</returns>
        public bool Equals(ByteRange other)
        {
            return Start == other.Start && End == other.End;
        }

        /// <summary>返回此实例的哈希代码。</summary>
        /// <returns>一个 32 位带符号整数，它是此实例的哈希代码。</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }
    }
}