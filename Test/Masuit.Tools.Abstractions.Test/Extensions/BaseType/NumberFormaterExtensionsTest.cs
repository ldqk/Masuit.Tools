using System.Numerics;
using Masuit.Tools.Strings;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions.BaseType;

public class NumberFormaterExtensionsTest
{
    [Fact]
    public static void CanToBase36()
    {
        Assert.Equal(36.ToBase(36), "10");
        Assert.Equal(36, "10".FromBase(36));
        Assert.Equal(36l.ToBase(36), "10");
        Assert.Equal(new BigInteger(36).ToBase(36), "10");
        Assert.Equal(new BigInteger(36), "10".FromBaseBig(36));
        var formater = new UnicodeFormater("😀😁😂🤣😃😄😅😆😉😊😋😎😍😘🥰😗😙🥲😚🙂🤗🤩🤔🤨😑😶😶‍🌫🙄😏😣😥😮");
        Assert.Equal(formater.ToString(1234567890), "😁😃😶😍😀🤔😚");
        Assert.Equal(formater.FromString("😁😃😶😍😀🤔😚"), 1234567890);
        Assert.Equal(formater.FromStringBig("😁😃😶😍😀🤔😚"), 1234567890);
    }
}