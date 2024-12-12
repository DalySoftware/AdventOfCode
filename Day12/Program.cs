// See https://aka.ms/new-console-template for more information

// var input = """
//             AAAA
//             BBCD
//             BBCC
//             EEEC
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

    var currentFences = 0;
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

            currentFences += map.Neighbours(plot).Count(n => n.Value != plot.Value);
            currentArea += 1;

            foreach (var neighbour in neighbours.Where(n => n.Value == plot.Value))
                currentGroup.Push(neighbour);
        }


        if (currentGroup.Count == 0) StartNewGroup();
    }

    return totalCost;

    void StartNewGroup()
    {
        totalCost += currentFences * currentArea;
        Console.WriteLine(currentFences + " * " + currentArea);

        currentFences = 0;
        currentArea = 0;

        var next = plots.Except(seen).FirstOrDefault(p => p.Value is not null);
        if (next != null) currentGroup.Push(next);
    }
}


record Map(HashSet<Plot> Plots)
{
    internal IEnumerable<Plot> Neighbours(Plot plot)
        => Plots.Where(other => other.IsNeighbour(plot));
}

record Plot(char? Value, int X, int Y)
{
    internal bool IsNeighbour(Plot other) =>
        (X == other.X && Math.Abs(Y - other.Y) == 1) || (Y == other.Y && Math.Abs(X - other.X) == 1);
}