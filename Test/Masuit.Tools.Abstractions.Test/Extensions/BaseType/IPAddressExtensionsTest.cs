using System.Net;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions.BaseType;

public class IPAddressExtensionsTest
{
    [Fact]
    public void Can_IsPrivateIP()
    {
        Assert.True(IPAddress.Parse("192.168.31.13").IsPrivateIP());
    }
}