// See https://aka.ms/new-console-template for more information

// var input = """
//             5,4
//             4,2
//             4,5
//             3,0
//             2,1
//             6,3
//             2,4
//             1,5
//             0,6
//             3,3
//             2,6
//             5,1
//             1,2
//             5,5
//             2,5
//             6,5
//             1,4
//             0,4
//             6,4
//             1,1
//             6,1
//             1,0
//             0,5
//             1,6
//             2,0
//             """;

var input = File.ReadAllText("input.txt");

var map = new Map(Parse(input, int.MaxValue));

var result = map.FirstBlockingByte();

Console.WriteLine("Result:");
Console.WriteLine(result);

return;

Entity[] Parse(string input, int limit) => input
    .Split(["\n", "\r\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
    .Select(
        line => line.Split(",").Select(int.Parse).ToArray() switch
        {
            [var x, var y] => new Byte(new Position(x, y)),
            _ => throw new ArgumentException("Too many coordinates"),
        })
    .Take(limit)
    .Append<Entity>(new Player(new(0, 0)))
    .ToArray();

record Position(int X, int Y);

abstract record Entity(Position Position);

record Player(Position Position) : Entity(Position);

record Byte(Position Position) : Entity(Position);

class Map(Entity[] entities)
{
    Entity[] Entities { get; } = entities;

    const int MaxX = 70;
    const int MaxY = 70;
    Position Exit => new(MaxX, MaxY);
    Player Player => Entities.OfType<Player>().Single();
    IEnumerable<Byte> AllBytes => Entities.OfType<Byte>();
    IEnumerable<Byte> Bytes => AllBytes.Take(_takeBytes);

    int _takeBytes = 1;

    internal Position FirstBlockingByte()
    {
        while (_takeBytes <= AllBytes.Count())
        {
            Console.WriteLine(_takeBytes);
            if (!CanReachExit())
            {
                var blocker = Bytes.Last().Position;
                Console.WriteLine(_takeBytes + " : " + blocker);
                return blocker;
            }

            _takeBytes++;
        }

        throw new Exception("No bytes blocked it");
    }

    bool CanReachExit()
    {
        var toProcess = new Stack<Player>();
        toProcess.Push(Player);

        HashSet<Player> visited = [];

        while (toProcess.TryPop(out var currentPlayer))
        {
            // Console.WriteLine(current);

            if (!visited.Add(currentPlayer)) continue;
            if (currentPlayer.Position == Exit) return true;
            foreach (var newPosition in Neighbours(currentPlayer.Position))
                toProcess.Push(new Player(newPosition));
        }

        return false;
    }


    IEnumerable<Position> Neighbours(Position position) =>
        PossibleNeighbours(position)
            .Where(InBounds)
            .Where(p => !Blocked(p));

    IEnumerable<Position> PossibleNeighbours(Position position)
    {
        yield return position with { X = position.X - 1 };
        yield return position with { X = position.X + 1 };
        yield return position with { Y = position.Y - 1 };
        yield return position with { Y = position.Y + 1 };
    }


    bool InBounds(Position position) =>
        position.X >= 0 && position.X <= MaxX &&
        position.Y >= 0 && position.Y <= MaxY;

    bool Blocked(Position position) => Bytes.Any(b => b.Position == position);
};