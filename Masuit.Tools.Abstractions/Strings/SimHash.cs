using System.Linq;
using System.Numerics;

namespace Masuit.Tools.Strings;

public class SimHash
{
    private readonly string _tokens;
    private readonly BigInteger _strSimHash;
    private readonly int _hashBits = 128;

    public BigInteger StrSimHash => _strSimHash;

    public SimHash(string tokens, int hashBits)
    {
        _tokens = tokens;
        _hashBits = hashBits;
        _strSimHash = GetSimHash();
    }

    public SimHash(string tokens)
    {
        _tokens = tokens;
        _strSimHash = GetSimHash();
    }

    private BigInteger GetSimHash()
    {
        var v = new int[_hashBits];
        var stringTokens = new SimTokenizer(_tokens);
        while (stringTokens.HasMoreTokens())
        {
            var temp = stringTokens.NextToken();
            var t = Hash(temp);
            for (var i = 0; i < _hashBits; i++)
            {
                var bitmask = BigInteger.One << i;
                if ((t & bitmask).Sign != 0)
                {
                    v[i] += 1;
                }
                else
                {
                    v[i] -= 1;
                }
            }
        }

        var fingerprint = BigInteger.Zero;
        for (var i = 0; i < _hashBits; i++)
        {
            if (v[i] >= 0)
            {
                fingerprint += BigInteger.Parse("1") << i;
            }
        }

        return fingerprint;
    }

    private BigInteger Hash(string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return BigInteger.Zero;
        }

        var sourceArray = source.ToCharArray();
        var x = new BigInteger((long)sourceArray[0] << 7);
        var m = BigInteger.Parse("1000003");
        var mask = BigInteger.Pow(new BigInteger(2), _hashBits) - BigInteger.One;
        x = sourceArray.Select(item => new BigInteger((long)item)).Aggregate(x, (current, temp) => ((current * m) ^ temp) & mask);
        x ^= new BigInteger(source.Length);
        if (x.Equals(BigInteger.MinusOne))
        {
            x = new BigInteger(-2);
        }

        return x;
    }

    public int HammingDistance(SimHash other)
    {
        var m = (BigInteger.One << _hashBits) - BigInteger.One;
        var x = (_strSimHash ^ other._strSimHash) & m;
        var tot = 0;
        while (x.Sign != 0)
        {
            tot += 1;
            x &= x - BigInteger.One;
        }

        return tot;
    }
}

//简单的分词法，直接将中文分成单个汉字。可以用其他分词法代替
public class SimTokenizer
{
    private readonly string _source;
    private int _index;
    private readonly int _length;

    public SimTokenizer(string source)
    {
        _source = source;
        _index = 0;
        _length = (source ?? "").Length;
    }

    public bool HasMoreTokens()
    {
        return _index < _length;
    }

    public string NextToken()
    {
        var s = _source.Substring(_index, 1);
        _index++;
        return s;
    }
}
