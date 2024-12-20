// See https://aka.ms/new-console-template for more information

// var input = """
//             ###############
//             #...#...#.....#
//             #.#.#.#.#.###.#
//             #S#...#.#.#...#
//             #######.#.#.###
//             #######.#.#...#
//             #######.#.###.#
//             ###..E#...#...#
//             ###.#######.###
//             #...###...#...#
//             #.#####.#.###.#
//             #.#...#.#.#...#
//             #.#.#.#.#.#.###
//             #...#...#...###
//             ###############
//             """;

var input = File.ReadAllText("input.txt");

var map = Parse(input);

var cheats = map.GetCheats(100);

foreach (var group in cheats.OrderBy(g => g.Key))
    Console.WriteLine($"There are {group.Count()} cheats that save {group.Key} picoseconds");

Console.WriteLine("Result:");
Console.WriteLine(cheats.SelectMany(kv => kv).Count());

return;

Map Parse(string input) =>
    new(input
        .Split(["\n", "\r\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select((line, index) => (line, yIndex: index))
        .SelectMany(a => a.line
            .Select<char, Entity?>((ch, xIndex) =>
                ch switch
                {
                    '#' => new Wall(new Position(xIndex, a.yIndex)),
                    'S' => new Start(new Position(xIndex, a.yIndex)),
                    'E' => new End(new Position(xIndex, a.yIndex)),
                    _ => null,
                }))
        .Where(e => e != null)
        .ToArray()!);

readonly record struct Position(int X, int Y)
{
    internal int NewYorkDistance(Position other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
};


abstract record Entity(Position Position);

record Wall(Position Position) : Entity(Position);

record Start(Position Position) : Entity(Position);

record End(Position Position) : Entity(Position);

record Cheat(Position Start, Position End, int Saving);

class Map(Entity[] entities)
{
    HashSet<Position> Walls { get; } = entities.OfType<Wall>().Select(s => s.Position).ToHashSet();
    End End { get; } = entities.OfType<End>().Single();

    internal ILookup<int, Cheat> GetCheats(int minSaving)
    {
        var dictionary = TimesToEnd();
        HashSet<Cheat> allCheats = [];
        foreach (var kv in dictionary)
        {
            var (position, distance) = kv;
            var reachable = CachedNthNeighbours(position, 20);

            var cheats = reachable.Select(n =>
                {
                    var saving = dictionary[n] - (distance + n.NewYorkDistance(position));
                    return new Cheat(position, n, saving);
                })
                .Where(c => c.Saving >= minSaving);
            allCheats.UnionWith(cheats);
        }

        return allCheats.ToLookup(c => c.Saving);
    }

    readonly Dictionary<Position, Position[]> _cachedNthNeighbours = new();

    Position[] CachedNthNeighbours(Position position, int n) =>
        _cachedNthNeighbours.TryGetValue(position, out var cached)
            ? cached
            : _cachedNthNeighbours[position] = NthNeighbours(position, n).ToArray();

    IEnumerable<Position> NthNeighbours(Position position, int n)
    {
        var startX = Math.Max(0, position.X - n);
        var endX = Math.Min(MaxX, position.X + n);
        var startY = Math.Max(0, position.Y - n);
        var endY = Math.Min(MaxY, position.Y + n);

        return Enumerable.Range(startX, endX - startX + 1)
            .SelectMany(x => Enumerable.Range(startY, endY - startY + 1)
                .Select(y => new Position(x, y)))
            .Where(p => p.NewYorkDistance(position) <= n)
            .Where(p => !Walls.Contains(p));
    }

    Dictionary<Position, int> TimesToEnd()
    {
        Dictionary<Position, int> dictionary = new();
        Stack<(Position, int)> toProcess = [];

        toProcess.Push((End.Position, 0));

        while (toProcess.TryPop(out var current))
        {
            var (position, distance) = current;
            if (!dictionary.TryAdd(position, distance)) continue;

            var neighbours = UnblockedNeighbours(position);
            foreach (var neighbour in neighbours) toProcess.Push((neighbour, distance + 1));
        }

        return dictionary;
    }


    IEnumerable<Position> UnblockedNeighbours(Position position) =>
        InBoundsNeighbours(position).Where(p => !Blocked(p));

    IEnumerable<Position> InBoundsNeighbours(Position position) => CandidateNeighbours(position).Where(InBounds);

    static IEnumerable<Position> CandidateNeighbours(Position position)
    {
        yield return position with { X = position.X - 1 };
        yield return position with { X = position.X + 1 };
        yield return position with { Y = position.Y - 1 };
        yield return position with { Y = position.Y + 1 };
    }

    int MaxX => entities.Max(e => e.Position.X);
    int MaxY => entities.Max(e => e.Position.Y);

    bool InBounds(Position position) =>
        position.X >= 0 && position.X <= MaxX &&
        position.Y >= 0 && position.Y <= MaxY;

    bool Blocked(Position position) => Walls.Contains(position);
}