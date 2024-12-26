using System.Collections.Generic;
using Masuit.Tools.Abstractions.Test.Tree;
using Xunit;

namespace Masuit.Tools.Database;

/// <summary>
/// DataTable帮助类
/// </summary>
public class DataTableHelperTest
{
    [Fact]
    public void CanListToDataTable()
    {
        var list = new List<MyClass3>
        {
            new MyClass3()
            {
                Id = 1,
                ParentId = 1
            }
        };
        var table = list.ToDataTable("Test");
        table.AddIdentityColumn();
        var hasRows = table.HasRows();
        var source = table.ToList<MyClass3>();
        var dataRows = table.Rows.GetDataRowArray();

        Assert.True(hasRows);
        Assert.Equal(1, source.Count);
        Assert.Equal(1, dataRows.Length);
        Assert.False(new List<MyClass3>().ToDataTable("Test").HasRows());
    }
}