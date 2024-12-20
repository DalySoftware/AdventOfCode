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

var cheats = map.GetCheats();

foreach (var group in cheats.OrderBy(g => g.Key))
    Console.WriteLine($"There are {group.Count()} cheats that save {group.Key} picoseconds");

Console.WriteLine("Result:");
Console.WriteLine(cheats.Where(kv => kv.Key >= 100).SelectMany(kv => kv).Count());

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

record struct Position(int X, int Y);


abstract record Entity(Position Position);

record Wall(Position Position) : Entity(Position);

record Start(Position Position) : Entity(Position);

record End(Position Position) : Entity(Position);

record Cheat(Position Start, Position End, int Saving);

class Map(Entity[] entities)
{
    IEnumerable<Wall> Walls => entities.OfType<Wall>();
    Start Start => entities.OfType<Start>().Single();
    End End => entities.OfType<End>().Single();

    internal ILookup<int, Cheat> GetCheats()
    {
        var dictionary = TimesToEnd();
        HashSet<Cheat> allCheats = [];
        foreach (var kv in dictionary)
        {
            var (position, distance) = kv;
            var immediateNeighbours = InBoundsNeighbours(position);
            var twiceNeighbours =
                immediateNeighbours
                    .SelectMany(InBoundsNeighbours)
                    .Where(n => n != position)
                    .Where(n => Walls.All(w => w.Position != n))
                    .Distinct();

            var cheats = twiceNeighbours.Select(n =>
                {
                    var saving = dictionary[n] - (distance + 2);
                    return new Cheat(position, n, saving);
                })
                .Where(c => c.Saving > 0);
            foreach (var cheat in cheats) allCheats.Add(cheat);
        }

        return allCheats.ToLookup(c => c.Saving);
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

    bool Blocked(Position position) => Walls.Any(b => b.Position == position);
}