using System;

namespace Masuit.Tools.Mapping.Core
{
    internal struct TypePairMapper : IEquatable<TypePairMapper>
    {
        public TypePairMapper(Type source, Type target, string name = null) : this()
        {
            Target = target;
            Source = source;
            Name = name;
        }

        public Type Source { get; }
        public Type Target { get; }

        public string Name { get; private set; }

        public static TypePairMapper Create(Type source, Type target, string name = null)
        {
            return new TypePairMapper(source, target, name);
        }

        public static TypePairMapper Create<TSource, TDest>(string name = null)
        {
            return new TypePairMapper(typeof(TSource), typeof(TDest), name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is TypePairMapper && Equals((TypePairMapper)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source != null ? Source.GetHashCode() : 0) * 397) ^ (Target != null ? Target.GetHashCode() : 0);
            }
        }
        public bool Equals(TypePairMapper other)
        {
            bool result;
            if (!string.IsNullOrEmpty(other.Name))
            {
                result = Source == other.Source && Target == other.Target && Name == other.Name;
            }
            else
            {
                result = Source == other.Source && Target == other.Target;
            }
            return result;
        }
    }
}
