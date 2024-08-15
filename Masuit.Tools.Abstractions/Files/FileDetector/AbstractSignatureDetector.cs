using System.Reflection;
using System.Runtime.InteropServices;
using Masuit.Tools.Mime;

namespace Masuit.Tools.Files.FileDetector;

[StructLayout(LayoutKind.Sequential)]
public record struct SignatureInformation : IEquatable<SignatureInformation>
{
    /// <summary>
    ///
    /// </summary>
    public int Position{get;set;}

    /// <summary>
    ///
    /// </summary>
    public byte[] Signature{get;set;}

    /// <summary>
    ///
    /// </summary>
    public byte[] Presignature{get;set;}

    /// <summary>指示当前对象是否等于同一类型的另一个对象。</summary>
    /// <param name="other">一个与此对象进行比较的对象。</param>
    /// <returns>如果当前对象等于 <paramref name="other" /> 参数，则为 true；否则为 false。</returns>
    public bool Equals(SignatureInformation other)
    {
        return Position == other.Position && Signature.SequenceEqual(other.Signature) && Presignature.SequenceEqual(other.Presignature);
    }

    /// <summary>返回此实例的哈希代码。</summary>
    /// <returns>一个 32 位带符号整数，它是此实例的哈希代码。</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Position;
            hashCode = (hashCode * 397) ^ (Signature != null ? Signature.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Presignature != null ? Presignature.GetHashCode() : 0);
            return hashCode;
        }
    }
}

public abstract class AbstractSignatureDetector : IDetector
{
    public abstract string Extension { get; }

    public virtual string Precondition => null;

    protected abstract SignatureInformation[] SignatureInformations { get; }

    public virtual string MimeType => new MimeMapper().GetMimeFromExtension("." + Extension);

    public virtual List<FormatCategory> FormatCategories => GetType().GetCustomAttributes<FormatCategoryAttribute>().Select(a => a.Category).ToList();

    private int _cachedLongestLength = -1;

    public virtual bool Detect(Stream stream)
    {
        if (_cachedLongestLength == -1)
        {
            foreach (var sig in SignatureInformations)
            {
                _cachedLongestLength = _cachedLongestLength < sig.Signature.Length ? sig.Signature.Length : _cachedLongestLength;
            }
        }

        byte[] buffer = new byte[_cachedLongestLength];
        byte[] preSignature = null;
        foreach (var siginfo in SignatureInformations.Where(si => CompareArray(si.Presignature, preSignature)).OrderBy(si => si.Position))
        {
            stream.Position = siginfo.Position;
            _ = stream.Read(buffer, 0, _cachedLongestLength);
            if (CompareArray(siginfo.Signature, buffer))
            {
                preSignature = siginfo.Signature;
                return true;
            }
        }

        return false;
    }

    private static bool CompareArray(byte[] a1, byte[] a2)
    {
        if (a1 == null && a2 == null)
        {
            return true;
        }

        if (a1 == null || a2 == null)
        {
            return false;
        }
        return a1.Zip(a2,(x, y) => (x,y)).All(t => t.x==t.y);
    }
}