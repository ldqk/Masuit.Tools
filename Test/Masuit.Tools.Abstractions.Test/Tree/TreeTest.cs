using System;
using System.Collections.Generic;
using System.Linq;
using Masuit.Tools.Models;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Tree;

public class TreeTest
{
    [Fact]
    public void Can_BuildTree()
    {
        // arrange
        List<MyClass> list = new()
        {
            new MyClass
            {
                Name = "Root",
                Id = 1
            },
            new MyClass
            {
                Name = "Root",
                Id = 20000
            }
        };
        for (int i = 2; i < 1500; i++)
        {
            list.Add(new MyClass
            {
                Name = $"这是第{i}个子节点",
                Id = i,
                ParentId = i - 1
            });
        }

        for (int i = 20001; i < 40000; i++)
        {
            list.Add(new MyClass
            {
                Name = $"这是第{i}个子节点",
                Id = i,
                ParentId = i - 1
            });
        }

        // act
        List<MyClass> tree = list.ToTree();

        // assert
        Assert.Equal(tree[0].Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Id, 8);
        Assert.Equal(tree.Count, 2);
        Assert.Equal(tree[0].AllChildren().Count, 1498);
        List<MyClass> a = tree.Filter(c => c.Id == 39999).ToList();
        Assert.Equal(a[0].Id, 39999);
        List<MyClass> raw = tree.Flatten(c => c.Children).ToList();
        Assert.Equal(raw.Count, list.Count);
        List<MyClass> allParent = a[0].AllParent();
        Assert.Equal(allParent[0].AllChildren().Count, 19999);
        Assert.Equal(a[0].Root(), list[1]);
        Assert.StartsWith("Root", a[0].Path());
        Assert.Equal(a[0].Level(), 20000);
    }

    [Fact]
    public void Can_BuildTree2()
    {
        // arrange
        List<MyClass2> list = new()
        {
            new MyClass2
            {
                Name = "Root",
                Id = "1"
            },
            new MyClass2
            {
                Name = "Root",
                Id = "20000"
            }
        };
        for (int i = 2; i < 1500; i++)
        {
            list.Add(new MyClass2
            {
                Name = $"这是第{i}个子节点",
                Id = i.ToString(),
                ParentId = (i - 1).ToString()
            });
        }

        for (int i = 20001; i < 40000; i++)
        {
            list.Add(new MyClass2
            {
                Name = $"这是第{i}个子节点",
                Id = i.ToString(),
                ParentId = (i - 1).ToString()
            });
        }

        // act
        List<MyClass2> tree = list.ToTree();

        // assert
        Assert.Equal(tree[0].Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Id, "8");
        Assert.Equal(tree.Count, 2);
        Assert.Equal(tree[0].AllChildren().Count, 1498);
        List<MyClass2> a = tree.Filter(c => c.Id == "39999").ToList();
        Assert.Equal(a[0].Id, "39999");
        List<MyClass2> raw = tree.Flatten().ToList();
        Assert.Equal(raw.Count, list.Count);
        List<MyClass2> allParent = a[0].AllParent();
        Assert.Equal(allParent[0].AllChildren().Count, 19999);
        Assert.Equal(a[0].Root(), list[1]);
        Assert.StartsWith("Root", a[0].Path());
        Assert.Equal(a[0].Level(), 20000);
    }

    [Fact]
    public void Can_BuildTree3()
    {
        // 0-1-3
        //    -4-5
        //  -2
        MyClass3 tree0 = new()
        {
            Id = 0,
            ParentId = -1,
        };
        MyClass3 tree1 = new()
        {
            Id = 1,
            ParentId = 0,
        };
        MyClass3 tree2 = new()
        {
            Id = 2,
            ParentId = 0,
        };
        MyClass3 tree3 = new()
        {
            Id = 3,
            ParentId = 1,
        };
        MyClass3 tree4 = new()
        {
            Id = 4,
            ParentId = 1,
        };
        MyClass3 tree5 = new()
        {
            Id = 5,
            ParentId = 4,
        };

        // 准备数据
        List<MyClass3> list = new() { tree0, tree1, tree2, tree3, tree4, tree5 };

        // 执行
        List<Tree<MyClass3>> nodes = list.ToTreeGeneral(c => c.Id, c => c.ParentId);
        var count = nodes.Flatten().Count();
        //验证
        Assert.NotNull(nodes);
        Assert.Equal(5, count);  // 错误，返回的节点数为 2
    }
}

internal class MyClass : ITree<MyClass>, ITreeEntity<MyClass, int>
{
    /// <summary>
    /// 父节点
    /// </summary>
    public MyClass Parent { get; set; }

    /// <summary>
    /// 子级
    /// </summary>
    public ICollection<MyClass> Children { get; set; }

    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 主键id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 父级id
    /// </summary>
    public int? ParentId { get; set; }
}

internal class MyClass2 : ITree<MyClass2>, ITreeEntity<MyClass2>
{
    /// <summary>
    /// 父节点
    /// </summary>
    public MyClass2 Parent { get; set; }

    /// <summary>
    /// 子级
    /// </summary>
    public ICollection<MyClass2> Children { get; set; }

    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 主键id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 父级id
    /// </summary>
    public string ParentId { get; set; }
}

internal class MyClass3
{
    public long Id { get; set; }

    public long ParentId { get; set; }
}