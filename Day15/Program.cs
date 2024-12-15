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
                    '#' => new Boundary(new Position(xIndex, a.yIndex)),
                    'O' => new Box(new Position(xIndex, a.yIndex)),
                    '@' => new Robot(new Position(xIndex, a.yIndex)),
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
    Entity? EntityAt(Position position) => Entities.SingleOrDefault(e => e.Position == position);

    Robot GetRobot => Entities.OfType<Robot>().Single();

    internal void ExecuteRobotInstructions()
    {
        foreach (var instruction in Instructions) TryMove(GetRobot, instruction);
    }

    internal int TotalBoxDistances => Entities.OfType<Box>().Sum(b => b.Position.Distance);

    bool TryMove(Entity entity, Direction direction)
    {
        if (entity is Boundary) return false;

        var newPosition = entity.Position.NextIn(direction);
        var entityInWay = EntityAt(newPosition);

        if (entityInWay == null)
        {
            entity.Position = newPosition;
            return true;
        }

        if (!TryMove(entityInWay, direction)) return false;

        entity.Position = newPosition;
        return true;
    }

    internal void Render()
    {
        Console.WriteLine();
        Console.WriteLine();
        for (var x = 0; x < Entities.Max(e => e.Position.X); x++)
        {
            for (var y = 0; y < Entities.Max(e => e.Position.Y); y++)
            {
                var entity = EntityAt(new(x, y));
                var ch = entity switch
                {
                    Boundary => '#',
                    Robot => '@',
                    Box => 'O',
                    _ => '.',
                };

                Console.Write(ch);
            }

            Console.Write(Environment.NewLine);
        }
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

abstract class Entity(Position position)
{
    internal Position Position { get; set; } = position;
};

class Box(Position position) : Entity(position);

class Robot(Position position) : Entity(position);

class Boundary(Position position) : Entity(position);

enum Direction
{
    Up,
    Down,
    Left,
    Right,
}