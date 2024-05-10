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
        var list = new List<MyClass>()
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
                ParentId = (i - 1)
            });
        }

        for (int i = 20001; i < 40000; i++)
        {
            list.Add(new MyClass
            {
                Name = $"这是第{i}个子节点",
                Id = i,
                ParentId = (i - 1)
            });
        }

        // act
        var tree = list.ToTree();

        // assert
        Assert.Equal(tree[0].Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Id, 8);
        Assert.Equal(tree.Count, 2);
        Assert.Equal(tree[0].AllChildren().Count, 1498);
        var a = tree.Filter(c => c.Id == 39999).ToList();
        Assert.Equal(a[0].Id, 39999);
        var raw = tree.Flatten().ToList();
        Assert.Equal(raw.Count, list.Count);
        var allParent = a[0].AllParent();
        Assert.Equal(allParent[0].AllChildren().Count, 19999);
        Assert.Equal(a[0].Root(), list[1]);
        Assert.StartsWith("Root", a[0].Path());
        Assert.Equal(a[0].Level(), 20000);
    }

    [Fact]
    public void Can_BuildTree2()
    {
        // arrange
        var list = new List<MyClass2>()
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
        var tree = list.ToTree();

        // assert
        Assert.Equal(tree[0].Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Children.FirstOrDefault().Id, "8");
        Assert.Equal(tree.Count, 2);
        Assert.Equal(tree[0].AllChildren().Count, 1498);
        var a = tree.Filter(c => c.Id == "39999").ToList();
        Assert.Equal(a[0].Id, "39999");
        var raw = tree.Flatten().ToList();
        Assert.Equal(raw.Count, list.Count);
        var allParent = a[0].AllParent();
        Assert.Equal(allParent[0].AllChildren().Count, 19999);
        Assert.Equal(a[0].Root(), list[1]);
        Assert.StartsWith("Root", a[0].Path());
        Assert.Equal(a[0].Level(), 20000);
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