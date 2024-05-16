using BenchmarkDotNet.Attributes;
using Masuit.Tools.Models;

namespace BenchmarkTest;

[MemoryDiagnoser]
public class TreeTest
{
    public List<MyClass> Tree { get; set; }
    public List<MyClass> List { get; set; }

    public TreeTest()
    {
        List = new List<MyClass>()
        {
            new MyClass
            {
                Name = "Root",
                Id = 1
            }
        };
        for (int i = 2; i < 2000; i++)
        {
            List.Add(new MyClass
            {
                Name = $"这是第{i}个子节点",
                Id = i,
                ParentId = (i - 1)
            });
        }

        Tree = List.ToTree();
    }

    [Benchmark]
    public void BuildTree()
    {
        _ = List.ToTree();
    }

    [Benchmark]
    public void FilterNode()
    {
        var nodes = Tree.Filter(x => x.Name.Contains("514")).ToList();
    }

    [Benchmark]
    public void FlattenNode()
    {
        var nodes = List[1500].Flatten().ToList();
    }

    [Benchmark]
    public void FindRoot()
    {
        var root = List[1990].Root();
    }

    [Benchmark]
    public void FindAllChildren()
    {
        var children = List[1990].AllChildren();
    }

    [Benchmark]
    public void FindAllParents()
    {
        var children = List[1990].AllParent();
    }

    [Benchmark]
    public void FindLevel()
    {
        var children = List[1990].Level();
    }

    [Benchmark]
    public void GetPath()
    {
        var children = List[1990].Path();
    }
}

public class MyClass : ITree<MyClass>, ITreeEntity<MyClass, int>
{
    /// <summary>
    /// 子级
    /// </summary>
    public ICollection<MyClass> Children { get; set; }

    /// <summary>
    /// 主键id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 父级id
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 父节点
    /// </summary>
    public MyClass Parent { get; set; }

    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
}