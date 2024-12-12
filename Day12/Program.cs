// See https://aka.ms/new-console-template for more information

// var input = """
//             AAAA
//             BBCD
//             BBCC
//             EEEC
//             """;

// var input = """
//             AAAAAA
//             AAABBA
//             AAABBA
//             ABBAAA
//             ABBAAA
//             AAAAAA
//             """;

var input = File.ReadAllText("input.txt");

var map = Parse(input);
var cost = CalculateCost(map);

Console.WriteLine("Cost:");
Console.WriteLine(cost);

return;


Map Parse(string input)
{
    var lines = input.Split(["\r\n", "\n"],
        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    var plots = lines
        .Select((line, yIndex) => (line, yIndex))
        .SelectMany(x => x.line.Select((ch, xIndex) => new Plot(ch, xIndex, x.yIndex)))
        .ToHashSet();

    var maxX = lines[0].Length - 1;
    var maxY = lines.Length - 1;

    List<Plot> nullPerimeter = [];

    // Top and bottom perimeter
    for (var x = 0; x <= maxX; x++)
    {
        nullPerimeter.Add(new Plot(null, x, -1));
        nullPerimeter.Add(new Plot(null, x, maxY + 1));
    }

    // Left and right perimeter
    for (var y = 0; y <= maxY; y++)
    {
        nullPerimeter.Add(new Plot(null, -1, y));
        nullPerimeter.Add(new Plot(null, maxX + 1, y));
    }

    plots.UnionWith(nullPerimeter);

    return new Map(plots);
}


int CalculateCost(Map map)
{
    var plots = map.Plots.ToHashSet(); // take a copy
    var seen = new HashSet<Plot>();

    var currentFences = new HashSet<Fence>();
    var currentArea = 0;
    var totalCost = 0;

    var currentGroup = new Stack<Plot>();
    currentGroup.Push(plots.First(p => p.Value is not null));

    while (currentGroup.TryPop(out var plot))
    {
        if (!seen.Contains(plot))
        {
            var neighbours = map.Neighbours(plot).ToList();
            seen.Add(plot);

            foreach (var fence in map.Fences(plot)) currentFences.Add(fence);
            currentArea += 1;

            foreach (var neighbour in neighbours.Where(n => n.Value == plot.Value))
                currentGroup.Push(neighbour);
        }


        if (currentGroup.Count == 0) StartNewGroup();
    }

    return totalCost;

    void StartNewGroup()
    {
        var grouping = new FenceGrouping(currentFences.ToList());
        var groups = grouping.GroupByAdjacency();
        var sides = groups.Count;

        totalCost += sides * currentArea;
        Console.WriteLine(sides + " * " + currentArea);

        currentFences.Clear();
        currentArea = 0;

        var next = plots.Except(seen).FirstOrDefault(p => p.Value is not null);
        if (next != null) currentGroup.Push(next);
    }
}


record Map(HashSet<Plot> Plots)
{
    internal IEnumerable<Plot> Neighbours(Plot plot)
        => Plots.Where(other => other.IsNeighbour(plot));

    internal HashSet<Fence> Fences(Plot plot)
    {
        return Neighbours(plot)
            .Where(other => other.Value != plot.Value)
            .Select(other => new Fence(plot, other))
            .ToHashSet();
    }
}

record Fence
{
    decimal _x;
    decimal _y;

    char? _left;
    char? _right;
    char? _top;
    char? _bottom;

    internal Fence(Plot a, Plot b)
    {
        _x = (a.X + b.X) / 2m;
        _y = (a.Y + b.Y) / 2m;

        if (XIsHalf && YIsHalf) throw new InvalidOperationException();

        if (XIsHalf)
        {
            _left = a.X < b.X ? a.Value : b.Value;
            _right = a.X < b.X ? b.Value : a.Value;
        }

        if (YIsHalf)
        {
            _top = a.Y < b.Y ? a.Value : b.Value;
            _bottom = a.Y < b.Y ? b.Value : a.Value;
        }
    }

    static bool IsHalf(decimal num) => Math.Abs(num - Math.Floor(num) - 0.5m) == 0m;
    bool XIsHalf => IsHalf(_x);
    bool YIsHalf => IsHalf(_y);

    internal bool IsAdjacent(Fence other) =>
        (YIsHalf && (_top == other._top || _bottom == other._bottom) && _y == other._y && Math.Abs(_x - other._x) <= 1)
        ||
        (XIsHalf && (_left == other._left || _right == other._right) && _x == other._x && Math.Abs(_y - other._y) <= 1);
}

class FenceGrouping(List<Fence> fences)
{
    readonly bool[] _visited = new bool[fences.Count];

    public List<List<Fence>> GroupByAdjacency()
    {
        var groups = new List<List<Fence>>();
        for (var i = 0; i < fences.Count; i++)
            if (!_visited[i])
            {
                var group = new List<Fence>();
                DepthFirstSearch(i, group);
                groups.Add(group);
            }

        return groups;
    }

    void DepthFirstSearch(int index, List<Fence> group)
    {
        _visited[index] = true;
        group.Add(fences[index]);
        for (var i = 0; i < fences.Count; i++)
            if (!_visited[i] && fences[index].IsAdjacent(fences[i]))
                DepthFirstSearch(i, group);
    }
}

record Plot(char? Value, int X, int Y)
{
    internal bool IsNeighbour(Plot other) =>
        (X == other.X && Math.Abs(Y - other.Y) == 1) || (Y == other.Y && Math.Abs(X - other.X) == 1);
}