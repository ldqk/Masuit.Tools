using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    /// <summary>
    /// 路由计算引擎
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RouteEngine<T>
    {
        /// <summary>
        /// 节点信息
        /// </summary>
        private List<Node<T>> Nodes { get; set; }

        /// <summary>
        /// 路由结果
        /// </summary>
        private Dictionary<string, int> RouteList { get; set; } = new Dictionary<string, int>();

        public RouteEngine(List<Node<T>> nodes)
        {
            Nodes = nodes;
        }

        /// <summary>
        /// 递归将每条路都计算出来
        /// </summary>
        /// <param name="node"></param>
        /// <param name="route"></param>
        /// <param name="dis"></param>
        private void InterateRoute(Node<T> node, string route, int dis)
        {
            if (node.Prevs.Any())
            {
                foreach (var prev in node.Prevs)
                {
                    RouteList[prev.Key.Name + "," + route] = dis + prev.Value;
                    InterateRoute(prev.Key, prev.Key.Name + "," + route, dis + prev.Value);
                }
            }
        }

        /// <summary>
        /// 获得路径
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="shortest"></param>
        /// <returns></returns>
        public (List<Route<T>>, HashSet<Node<T>>) GetRoutes(Node<T> start, Node<T> end, bool shortest)
        {
            InterateRoute(end, end.Name, 0);
            var list = new List<Route<T>>();
            var nodes = new HashSet<Node<T>>();
            var routes = RouteList.Where(k => k.Key.StartsWith(start.Name) && k.Key.EndsWith(end.Name));
            var route = (shortest ? routes.OrderBy(x => x.Value) : routes.OrderByDescending(x => x.Value)).FirstOrDefault().Key;
            string[] strs = route.Split(',');
            for (var i = 0; i < strs.Length - 1; i++)
            {
                Node<T> src = Nodes.Find(n => n.Name.Equals(strs[i]));
                Node<T> dest = Nodes.Find(n => n.Name.Equals(strs[i + 1]));
                list.Add(new Route<T>(src, dest, dest.Prevs[src]));
                nodes.Add(src);
                nodes.Add(dest);
            }
            return (list, nodes);
        }
    }

    /// <summary>
    /// 路由
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Route<T>
    {
        public Route(Node<T> src, Node<T> dest, int distance)
        {
            Source = src;
            Dest = dest;
            Distance = distance;
        }

        /// <summary>
        /// 开始节点
        /// </summary>
        public Node<T> Source { get; set; }

        /// <summary>
        /// 结束节点
        /// </summary>
        public Node<T> Dest { get; set; }

        /// <summary>
        /// 距离
        /// </summary>
        public int Distance { get; set; }
    }

    /// <summary>
    /// 节点
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Node<T>
    {
        public Node(string name)
        {
            Name = name;
            Prevs = new Dictionary<Node<T>, int>();
        }

        /// <summary>
        /// 节点名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 前面的节点以及到前一个节点需要的距离
        /// </summary>
        public Dictionary<Node<T>, int> Prevs { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Node<string> a = new Node<string>("A");
            Node<string> b = new Node<string>("B");
            Node<string> c = new Node<string>("C");
            Node<string> d = new Node<string>("D");
            Node<string> e = new Node<string>("E");
            SetRoutePath(a, b, 1);
            SetRoutePath(b, c, 2);
            SetRoutePath(a, c, 2);
            SetRoutePath(b, d, 3);
            SetRoutePath(c, d, 5);
            SetRoutePath(b, e, 9);
            SetRoutePath(d, e, 4);
            List<Node<string>> nodes = new List<Node<string>>()
            {
                a,
                b,
                c,
                d,
                e
            };
            var engine = new RouteEngine<string>(nodes);
            var (routes, routeNodes) = engine.GetRoutes(a, e, false);
            foreach (var x in routes)
            {
                Console.WriteLine(x.Source.Name + "->" + x.Dest.Name + ":" + x.Distance);
            }

            Console.WriteLine("最长路径：" + string.Join("->", routeNodes.Select(x => x.Name)) + ":" + routes.Sum(r => r.Distance));

            (routes, routeNodes) = engine.GetRoutes(a, e, true);
            foreach (var x in routes)
            {
                Console.WriteLine(x.Source.Name + "->" + x.Dest.Name + ":" + x.Distance);
            }

            Console.WriteLine("最短路径：" + string.Join("->", routeNodes.Select(x => x.Name)) + ":" + routes.Sum(r => r.Distance));
        }

        private static void SetRoutePath(Node<string> start, Node<string> end, int distance)
        {
            end.Prevs.Add(start, distance);
        }
    }
}