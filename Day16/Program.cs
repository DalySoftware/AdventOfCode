// See https://aka.ms/new-console-template for more information

// var input = """
//             ###############
//             #.......#....E#
//             #.#.###.#.###.#
//             #.....#.#...#.#
//             #.###.#####.#.#
//             #.#.#.......#.#
//             #.#.#####.###.#
//             #...........#.#
//             ###.#.#####.#.#
//             #...#.....#.#.#
//             #.#.#.###.#.#.#
//             #.....#...#.#.#
//             #.###.#.#.#.#.#
//             #S..#.....#...#
//             ###############
//             """;

// var input = """
//             ######
//             #...E#
//             #S...#
//             ######
//             """;

var input = File.ReadAllText("input.txt");

var map = Parse(input);
// var score = map.MinimumScore();
var score = map.TilesOnBestPaths();

Console.WriteLine("Score:");
Console.WriteLine(score);

return;

Map Parse(string input)
{
    var entities = input.Split(["\r\n", "\n"],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(line => line.StartsWith('#'))
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
        .OfType<Entity>()
        .ToArray();

    return new Map(entities);
}


class Map(Entity[] entities)
{
    Entity[] Entities { get; } = entities;
    Start Start => Entities.OfType<Start>().Single();
    End End => Entities.OfType<End>().Single();
    IEnumerable<Wall> Walls => Entities.OfType<Wall>();

    internal int MinimumScore() => GetPathsToEnd().Min(p => p.Last().Score);

    internal int TilesOnBestPaths() =>
        GetPathsToEnd()
            .GroupBy(p => p.Last().Score)
            .OrderBy(g => g.Key)
            .First()
            .SelectMany(p => p.Select(r => r.Position))
            .Distinct()
            .Count();

    IEnumerable<Reindeer[]> GetPathsToEnd()
    {
        var toProcess = new Queue<(Reindeer, Reindeer[])>();
        var seenStates = new HashSet<Reindeer>();

        var startReinder = new Reindeer(Start.Position, Direction.East, 0);
        toProcess.Enqueue((startReinder, [startReinder]));

        while (toProcess.TryDequeue(out var current))
        {
            var (reindeer, history) = current;
            if (reindeer.Position == End.Position)
                // foreach (var path in current.Item2) Console.WriteLine(path);
                yield return history;

            if (seenStates.Any(s =>
                    s.Position == reindeer.Position && s.Direction == reindeer.Direction && s.Score < reindeer.Score))
                // Skip if we've already been here by a more efficient route.
                continue;

            seenStates.Add(reindeer);

            // Render(seenStates);

            if (!WithinBounds(reindeer.Position))
                continue;

            if (Walls.Any(w => w.Position == reindeer.Position))
                continue;

            Reindeer[] candidates =
            [
                reindeer.MoveForward(),
                reindeer.RotateClockwise(),
                reindeer.RotateAntiClockwise(),
            ];

            foreach (var candidate in candidates) toProcess.Enqueue((candidate, [..history, candidate]));
        }
    }

    bool _firstRender = true;

    void Render(HashSet<Reindeer> seenStates)
    {
        var wallPositions = Walls.Select(w => w.Position).ToHashSet();
        var seenPositions = seenStates.Select(s => s.Position).Except(wallPositions).ToHashSet();

        if (_firstRender)
        {
            Console.Clear();
            for (var y = 0; y <= MaxY; y++)
            {
                for (var x = 0; x <= MaxX; x++)
                {
                    var position = new Position(x, y);
                    if (seenPositions.Contains(position))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("x");
                    }
                    else if (wallPositions.Contains(position))
                    {
                        Console.Write("#");
                    }
                    else
                    {
                        Console.Write(".");
                    }

                    Console.ResetColor();
                }

                Console.Write(Environment.NewLine);
                _firstRender = false;
            }
        }

        Console.SetWindowSize(MaxX + 1, MaxY + 1);
#if WINDOWS
        Console.SetBufferSize(MaxX + 1, MaxY + 1);
#endif
        foreach (var position in seenPositions)
        {
            Console.SetCursorPosition(position.X, position.Y);
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("x");
            Console.ResetColor();
        }
    }

    int MaxX => Entities.Max(e => e.Position.X);
    int MaxY => Entities.Max(e => e.Position.Y);

    bool WithinBounds(Position position) =>
        position.X >= 0 && position.X <= MaxX && position.Y >= 0 && position.Y <= MaxY;
}

record struct Position(int X, int Y);

abstract record Entity(Position Position);

enum Direction
{
    North,
    East,
    South,
    West,
}

record Reindeer(Position Position, Direction Direction, int Score) : Entity(Position)
{
    int Mod(int num, int modulus) => (num % modulus + modulus) % modulus;

    internal Reindeer RotateClockwise() =>
        this with
        {
            Direction = (Direction)Mod((int)Direction + 1, 4),
            Score = Score + 1000,
        };

    internal Reindeer RotateAntiClockwise() =>
        this with
        {
            Direction = (Direction)Mod((int)Direction - 1, 4),
            Score = Score + 1000,
        };

    internal Reindeer MoveForward()
    {
        var velocity = Direction switch
        {
            Direction.North => (0, -1),
            Direction.East => (1, 0),
            Direction.South => (0, 1),
            Direction.West => (-1, 0),
            _ => throw new ArgumentOutOfRangeException(),
        };

        return this with
        {
            Position = new Position(Position.X + velocity.Item1, Position.Y + velocity.Item2),
            Score = Score + 1,
        };
    }
}

record Wall(Position Position) : Entity(Position);

record Start(Position Position) : Entity(Position);

record End(Position Position) : Entity(Position);