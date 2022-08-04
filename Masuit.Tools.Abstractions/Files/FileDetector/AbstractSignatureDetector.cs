using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Masuit.Tools.AspNetCore.Mime;

namespace Masuit.Tools.Files.FileDetector;

[StructLayout(LayoutKind.Sequential)]
public struct SignatureInformation
{
    /// <summary>
    ///
    /// </summary>
    public int Position;

    /// <summary>
    ///
    /// </summary>
    public byte[] Signature;

    /// <summary>
    ///
    /// </summary>
    public byte[] Presignature;
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
        bool correct = false;
        while (true)
        {
            bool found = false;
            foreach (var siginfo in SignatureInformations.Where(si => CompareArray(si.Presignature, preSignature)).OrderBy(si => si.Position))
            {
                correct = false;
                stream.Position = siginfo.Position;
                stream.Read(buffer, 0, _cachedLongestLength);
                if (CompareArray(siginfo.Signature, buffer))
                {
                    preSignature = siginfo.Signature;
                    correct = true;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                break;
            }
        }

        return correct;
    }

    private bool CompareArray(byte[] a1, byte[] a2)
    {
        if (a1 == null && a2 == null) return true;
        if (a1 == null || a2 == null) return false;

        bool failed = false;
        int min = a1.Length > a2.Length ? a2.Length : a1.Length;
        for (int i = 0; i < min; ++i)
        {
            if (a1[i] != a2[i])
            {
                failed = true;
                break;
            }
        }
        return !failed;
    }
}
