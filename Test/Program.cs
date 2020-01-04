using Masuit.Tools.RandomSelector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            var result = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                result.Add(new List<WeightedItem<string>>()
                {
                    new WeightedItem<string>("A", 1),
                    new WeightedItem<string>("B", 3),
                    new WeightedItem<string>("C", 4),
                    new WeightedItem<string>("D", 4),
                }.WeightedItem());
            }

            foreach (var g in result.GroupBy(s => s).OrderByDescending(g => g.Count()))
            {
                Console.WriteLine(g.Key + ":" + g.Count());
            }
        }
    }
}