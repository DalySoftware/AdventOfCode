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

Console.WriteLine("Password:");
Console.WriteLine(solver.Password());

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
        var rawSets = graph.SelectMany(x => SetsOf(n, x.Key));
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

    internal string Password()
    {
        var longest = BiggestNetwork();
        return string.Join(",", longest.Order());
    }

    HashSet<string> BiggestNetwork()
    {
        Console.WriteLine(nameof(BiggestNetwork) + " start");

        var toVisit = new HashSet<HashSet<string>>(Comparer);
        var visited = new HashSet<HashSet<string>>(Comparer);

        foreach (var node in AllNodes) toVisit.Add([node]);

        HashSet<string> biggest = [];

        while (toVisit.Count > 0)
        {
            var network = toVisit.First();

            if (!visited.Add(network))
            {
                toVisit.Remove(network);
                continue;
            }

            if (visited.Count % 1_000_000 == 0) Console.WriteLine(visited.Count);

            if (!IsInterconnected(network))
            {
                toVisit.Remove(network);
                continue;
            }

            if (network.Count > biggest.Count)
            {
                biggest = network;
                Console.WriteLine(biggest.Count);
            }

            var newNetworks = network
                .SelectMany(ImmediateNeighbours)
                .Select(neighbour => network.Append(neighbour).ToHashSet());

            foreach (var newNet in newNetworks) toVisit.Add(newNet);
            toVisit.Remove(network);
        }

        Console.WriteLine(nameof(BiggestNetwork) + " end ");
        return biggest;
    }

    bool IsInterconnected(HashSet<string> nodes) =>
        nodes.SelectMany(_ => nodes, (first, second) => (first, second)).All(x =>
            x.first == x.second || HasEdge(x.first, x.second));

    bool HasEdge(string node, string other) => graph[node].Contains(other) || graph[other].Contains(node);

    IEnumerable<string> AllNodes => graph.Select(g => g.Key)
        .Union(graph.SelectMany(g => g))
        .Distinct();

    IEnumerable<string> ImmediateNeighbours(string node) =>
        graph[node].Union(
            graph.Where(g => g.Contains(node)).Select(kv => kv.Key));


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