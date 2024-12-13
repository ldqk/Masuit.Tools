using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions.BaseType;

public class IEnumerableTest
{
    [Fact]
    public void Can_ICollection_AppendIf()
    {
        // arrange
        var list = new List<string>();

        // act
        list.AppendIf(true, "1");
        list.AppendIf(() => true, "2");

        // assert
        Assert.True(list.Count == 2);
    }

    [Fact]
    public void Can_Enumerable_AppendIf()
    {
        // arrange
        var enumerable = Enumerable.Range(1, 1);

        // act
        enumerable = enumerable.AppendIf(true, 2).AppendIf(() => true, 3);

        // assert
        Assert.Equal(enumerable.Count(), 3);
    }
}