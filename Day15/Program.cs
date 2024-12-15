// See https://aka.ms/new-console-template for more information

// var input = """
//             ########
//             #..O.O.#
//             ##@.O..#
//             #...O..#
//             #.#.O..#
//             #...O..#
//             #......#
//             ########
//
//             <^^>>>vv<v>>v<<
//             """;

var input = File.ReadAllText("input.txt");

var map = Parse(input);

map.Render();
map.ExecuteRobotInstructions();
map.Render();

var distance = map.TotalBoxDistances;

Console.WriteLine("Distance:");
Console.WriteLine(distance);
Console.WriteLine(map.Instructions.Length);

return;

Map Parse(string input)
{
    var lines = input.Split(["\r\n", "\n"],
        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var entities = lines
        .Where(line => line.StartsWith('#'))
        .Select((line, index) => (line, yIndex: index))
        .SelectMany(a => a.line
            .Select<char, Entity?>((ch, xIndex) =>
                ch switch
                {
                    '#' => new Boundary(new Position(xIndex * 2, a.yIndex)),
                    'O' => new Box(new Position(xIndex * 2, a.yIndex)),
                    '@' => new Robot(new Position(xIndex * 2, a.yIndex)),
                    _ => null,
                }))
        .Where(x => x != null)
        .ToArray();

    var instructions = lines
        .Where(line => !line.StartsWith('#'))
        .SelectMany(line => line.Select(ch => ch switch
        {
            '^' => Direction.Up,
            'v' => Direction.Down,
            '<' => Direction.Left,
            '>' => Direction.Right,
            _ => throw new ArgumentOutOfRangeException(nameof(ch), ch, null),
        }))
        .ToArray();

    return new Map(entities, instructions);
}

record Map(Entity[] Entities, Direction[] Instructions)
{
    Entity? EntityAt(Position position)
    {
        return Entities.FirstOrDefault(e
            => e.Position == position || (e.Width == 2 && e.Position.NextIn(Direction.Right) == position));
    }

    IEnumerable<Entity> EntitiesAt(Position position, int width)
    {
        var left = EntityAt(position);
        if (left is not null) yield return left;


        if (width == 1) yield break;

        var right = EntityAt(position.NextIn(Direction.Right));
        if (right is not null && right != left) yield return right;
    }

    Robot GetRobot => Entities.OfType<Robot>().Single();

    internal void ExecuteRobotInstructions()
    {
        foreach (var instruction in Instructions)
            TryMove(GetRobot, instruction);
    }

    internal int TotalBoxDistances => Entities.OfType<Box>().Sum(b =>
    {
        var distance = b.Position.Distance;
        return distance;
    });

    (HashSet<Entity> Movable, bool Failed) CheckIfCanMove(Entity entity, Direction direction)
    {
        if (entity is Boundary) return ([], true);

        var newPosition = entity.Position.NextIn(direction);
        var entitiesInWay = EntitiesAt(newPosition, entity.Width)
            .Where(inWay => inWay != entity) // Is not itself
            .ToArray();

        List<Entity> toMove = [entity];
        foreach (var inWay in entitiesInWay)
        {
            var canMove = CheckIfCanMove(inWay, direction);
            if (canMove.Failed) return ([], true);

            toMove.AddRange(canMove.Movable);
        }

        return (toMove.ToHashSet(), false);
    }

    void TryMove(Entity entity, Direction direction)
    {
        var results = CheckIfCanMove(entity, direction);
        if (results.Failed) return;

        foreach (var toMove in results.Movable) toMove.Position = toMove.Position.NextIn(direction);
    }

    internal void Render(Direction? currentInstruction = null)
    {
        Console.WriteLine();
        Console.WriteLine();
        for (var y = 0; y < Entities.Max(e => e.Position.Y) + 1; y++)
        {
            for (var x = 0; x < Entities.Max(e => e.Position.X) + 2; x++)
            {
                var entity = EntityAt(new(x, y));
                switch (entity)
                {
                    case Boundary:
                        Console.Write('#');
                        break;
                    case Robot:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write('@');
                        break;
                    case Box when entity.Position.X == x:
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write('[');
                        break;
                    case Box when entity.Position.X == x - 1:
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write(']');
                        break;
                    default:
                        Console.Write(' ');
                        break;
                }

                Console.ResetColor();
            }

            Console.Write(Environment.NewLine);
        }

        Console.WriteLine(currentInstruction.ToString());
    }
};

record Position(int X, int Y)
{
    internal int Distance => 100 * Y + X;

    internal Position NextIn(Direction direction) =>
        direction switch
        {
            Direction.Up => this with { Y = Y - 1 },
            Direction.Down => this with { Y = Y + 1 },
            Direction.Left => this with { X = X - 1 },
            Direction.Right => this with { X = X + 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
};

abstract class Entity(Position position, int width)
{
    internal Position Position { get; set; } = position;

    internal int Width { get; } = width;
};

class Box(Position position) : Entity(position, 2);

class Robot(Position position) : Entity(position, 1);

class Boundary(Position position) : Entity(position, 2);

enum Direction
{
    Up,
    Down,
    Left,
    Right,
}