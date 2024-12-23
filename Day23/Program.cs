// See https://aka.ms/new-console-template for more information

using Graph = System.Linq.ILookup<string, string>;

// var input = """
//             kh-tc
//             qp-kh
//             de-cg
//             ka-co
//             yn-aq
//             qp-ub
//             cg-tb
//             vc-aq
//             tb-ka
//             wh-tc
//             yn-cg
//             kh-ub
//             ta-co
//             de-co
//             tc-td
//             tb-wq
//             wh-td
//             ta-ka
//             td-qp
//             aq-cg
//             wq-ub
//             ub-vc
//             de-ta
//             wq-aq
//             wq-vc
//             wh-yn
//             ka-de
//             kh-ta
//             co-tc
//             wh-qp
//             tb-vc
//             td-yn
//             """;

var input = File.ReadAllText("input.txt");

var graph = Parse(input);

var solver = new Solver(graph);

Console.WriteLine("Result:");
Console.WriteLine(solver.MatchingSetsOf(3).Count);

return;

Graph Parse(string input) =>
    input.Split(["\n", "\r\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select(line =>
        {
            var parts = line.Split("-").ToArray();
            return (parts[0], parts[1]);
        })
        .ToLookup(x => x.Item1, x => x.Item2);

class Solver(Graph graph)
{
    internal HashSet<HashSet<string>> MatchingSetsOf(int n)
    {
        var rawSets = graph.SelectMany(x => SetsOf(n, x.Key)).ToArray();
        var combined = rawSets.ToHashSet(Comparer);
        return combined.Where(set => set.Any(node => node.StartsWith('t'))).ToHashSet();
    }

    HashSet<HashSet<string>> SetsOf(int n, string startNode)
    {
        var results = new HashSet<HashSet<string>>(Comparer);
        var toVisit = new Stack<(string Node, HashSet<string> History)>();

        toVisit.Push((startNode, []));

        while (toVisit.TryPop(out var current))
        {
            var (node, history) = current;

            if (history.Count == n)
            {
                if (IsInterconnected(history))
                    results.Add(history);
                continue;
            }

            foreach (var newNode in graph[node]) toVisit.Push((newNode, [..history, node]));
        }

        return results;
    }

    bool IsInterconnected(HashSet<string> nodes) =>
        nodes.SelectMany(_ => nodes, (first, second) => (first, second)).All(x =>
            x.first == x.second || graph[x.first].Contains(x.second) || graph[x.second].Contains(x.first));


    static IEqualityComparer<HashSet<string>> Comparer => new HashSetComparer();

    class HashSetComparer : IEqualityComparer<HashSet<string>>
    {
        public bool Equals(HashSet<string>? x, HashSet<string>? y)
        {
            if (x == null || y == null)
                return false;
            return x.SetEquals(y);
        }

        public int GetHashCode(HashSet<string> obj)
        {
            return obj.Aggregate(0, (current, str) => current ^ str.GetHashCode());
        }
    }
}