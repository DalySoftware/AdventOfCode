// See https://aka.ms/new-console-template for more information

using System.Collections;

// var input = """
//             ....#.....
//             .........#
//             ..........
//             ..#.......
//             .......#..
//             ..........
//             .#..^.....
//             ........#.
//             #.........
//             ......#...
//             """;
var input = File.ReadAllText("input.txt");

var initialMap = Read(input);
var initialGuardPosition = initialMap.Guard.Point;

_ = initialMap.CausesLoop();
var possibleExtraObstructions = initialMap.VisitedPoints.Where(p => p != initialGuardPosition);

// Console.WriteLine(possibleExtraObstructions.Count());

var possibleMaps = possibleExtraObstructions
    .Select(p => new Map(
        initialMap.Obstacles.Append(new Obstacle { Point = p }),
        new Guard { Point = initialGuardPosition },
        initialMap.MaxX,
        initialMap.MaxY));

Console.WriteLine("Calculating loops");
var possibleLoops = possibleMaps.AsParallel().Count(m => m.CausesLoop());

Console.WriteLine("Loops found:");
Console.WriteLine(possibleLoops);

return;

Map Read(string input)
{
    var lines = input.Split(["\n", "\r\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    var entities = lines.SelectMany(ReadLine).ToList();

    return new Map(entities.OfType<Obstacle>(), entities.OfType<Guard>().Single(), lines.Length - 1,
        lines[0].Length - 1);
}

IEnumerable<MapEntity> ReadLine(string line, int yPosition) => line
    .Select((ch, index) => (ch, index))
    .Where(x => x.ch != '.')
    .Select<(char ch, int index), MapEntity>(x =>
        x.ch switch
        {
            '#' => new Obstacle { Point = new Vector2(x.index, yPosition) },
            '^' => new Guard { Point = new Vector2(x.index, yPosition) },
            _ => throw new InvalidOperationException(),
        })
;


record struct Vector2(int X, int Y)
{
    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
};

class Velocities : IEnumerator<Vector2>
{
    static Vector2 Up => new(0, -1);
    static Vector2 Right => new(1, 0);
    static Vector2 Down => new(0, 1);
    static Vector2 Left => new(-1, 0);

    readonly List<Vector2> _velocities = [Up, Right, Down, Left];
    int _index;

    public bool MoveNext()
    {
        _index = (_index + 1) % 4;
        return true;
    }

    public void Reset() => _index = 0;

    public Vector2 Current => _velocities[_index];

    object IEnumerator.Current => Current;

    public void Dispose()
    {
    }
}

abstract class MapEntity
{
    internal required Vector2 Point { get; set; }
};

class Guard : MapEntity
{
    readonly Velocities _velocities = new();

    internal Vector2 Velocity => _velocities.Current;
    internal void Rotate() => _velocities.MoveNext();
    internal void MoveTo(Vector2 position) => Point = position;
};

class Obstacle : MapEntity;

record Map(IEnumerable<Obstacle> Obstacles, Guard Guard, int MaxX, int MaxY)
{
    readonly HashSet<(Vector2 Point, Vector2 Velocity)>
        _seenStates = [(Guard.Point, Guard.Velocity)];

    internal IEnumerable<Vector2> VisitedPoints => _seenStates.Select(s => s.Point).Distinct();

    bool MoveGuard()
    {
        var newPosition = GetNextPosition();

        if (IsOutOfBounds(newPosition)) return false;

        _seenStates.Add((Guard.Point, Guard.Velocity));
        Guard.MoveTo(newPosition);
        return true;
    }

    Vector2 GetNextPosition()
    {
        while (true)
        {
            var target = Guard.Point + Guard.Velocity;
            if (!IsBlocked(target)) return target;

            Guard.Rotate();
        }
    }

    bool IsBlocked(Vector2 position) =>
        Obstacles.Any(o => o.Point == position);

    bool IsOutOfBounds(Vector2 position) => position.X > MaxX || position.Y > MaxY;

    internal bool CausesLoop()
    {
        const int limit = 1_000_000;
        var left = limit;
        while (left > 0)
        {
            if (!MoveGuard()) return false;
            if (_seenStates.Contains((Guard.Point, Guard.Velocity))) return true;
            left--;
            if (left % 1_000_000 == 0) Console.WriteLine(left);
        }

        Console.WriteLine($"Gave up after {limit} non repeated states");
        return false;
    }
}