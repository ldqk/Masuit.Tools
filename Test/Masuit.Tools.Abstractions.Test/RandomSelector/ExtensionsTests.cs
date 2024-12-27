using System;
using System.Collections.Generic;
using System.Linq;
using Masuit.Tools.RandomSelector;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.RandomSelector;

public class ExtensionsTests
{
    [Fact]
    public void TotalWeight_ShouldReturnCorrectWeight()
    {
        var items = new List<WeightedItem<string>>
        {
            new WeightedItem<string>("A", 1),
            new WeightedItem<string>("B", 2),
            new WeightedItem<string>("C", 3)
        };
        var selector = new WeightedSelector<string>(items);

        int totalWeight = selector.TotalWeight();

        Assert.Equal(6, totalWeight);
    }

    [Fact]
    public void OrderByWeightDescending_ShouldReturnItemsInDescendingOrder()
    {
        var items = new List<WeightedItem<string>>
        {
            new WeightedItem<string>("A", 1),
            new WeightedItem<string>("B", 3),
            new WeightedItem<string>("C", 2)
        };
        var selector = new WeightedSelector<string>(items);

        var orderedItems = selector.OrderByWeightDescending();

        Assert.Equal("B", orderedItems[0].Value);
        Assert.Equal("C", orderedItems[1].Value);
        Assert.Equal("A", orderedItems[2].Value);
    }

    [Fact]
    public void OrderByWeightAscending_ShouldReturnItemsInAscendingOrder()
    {
        var items = new List<WeightedItem<string>>
        {
            new WeightedItem<string>("A", 3),
            new WeightedItem<string>("B", 1),
            new WeightedItem<string>("C", 2)
        };
        var selector = new WeightedSelector<string>(items);

        var orderedItems = selector.OrderByWeightAscending();

        Assert.Equal("B", orderedItems[0].Value);
        Assert.Equal("C", orderedItems[1].Value);
        Assert.Equal("A", orderedItems[2].Value);
    }

    [Fact]
    public void WeightedItem_ShouldReturnItemBasedOnWeight()
    {
        var items = new List<WeightedItem<string>>
        {
            new WeightedItem<string>("A", 1),
            new WeightedItem<string>("B", 2),
            new WeightedItem<string>("C", 3)
        };

        var selectedItem = items.WeightedItem();

        Assert.Contains(selectedItem, items.Select(i => i.Value));
    }

    [Fact]
    public void WeightedItems_ShouldReturnMultipleItemsBasedOnWeight()
    {
        var items = new List<WeightedItem<string>>
        {
            new WeightedItem<string>("A", 1),
            new WeightedItem<string>("B", 2),
            new WeightedItem<string>("C", 3)
        };

        var selectedItems = items.WeightedItems(2).ToList();

        Assert.Equal(2, selectedItems.Count);
        Assert.All(selectedItems, item => Assert.Contains(item, items.Select(i => i.Value)));
    }

    [Fact]
    public void WeightedItems_WithKeySelector_ShouldReturnMultipleItemsBasedOnWeight()
    {
        var items = new List<string> { "A", "B", "C" };
        Func<string, int> keySelector = s => s == "A" ? 1 : s == "B" ? 2 : 3;

        var selectedItems = items.WeightedItems(2, keySelector).ToList();

        Assert.Equal(2, selectedItems.Count);
        Assert.All(selectedItems, item => Assert.Contains(item, items));
    }

    [Fact]
    public void WeightedBy_WithKeySelector_ShouldReturnSingleItemBasedOnWeight()
    {
        var items = new List<string> { "A", "B", "C" };
        Func<string, int> keySelector = s => s == "A" ? 1 : s == "B" ? 2 : 3;

        var selectedItem = items.WeightedBy(keySelector);

        Assert.Contains(selectedItem, items);
    }
}