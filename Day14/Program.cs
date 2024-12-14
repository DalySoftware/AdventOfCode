// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;

// var input = """
//             p=0,4 v=3,-3
//             p=6,3 v=-1,-3
//             p=10,3 v=-1,2
//             p=2,0 v=2,-1
//             p=0,0 v=1,3
//             p=3,0 v=-2,-2
//             p=7,6 v=-1,-3
//             p=3,0 v=-1,-2
//             p=9,3 v=2,3
//             p=7,3 v=-1,2
//             p=2,4 v=2,-3
//             p=9,5 v=-3,-3
//             """;

Console.WriteLine(Mod(-5, 103));
Console.WriteLine(Mod(105, 103));

// var input = File.ReadAllText("input.txt");

// var map = new Map(Parse(input).ToArray());
// map.MoveRobots(100);

// Console.WriteLine("Safety factor: ");
// Console.WriteLine(map.SafetyFactor);

return;

IEnumerable<Robot> Parse(string str) => str
    .Split(["\n", "\r\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
    .Select(line =>
    {
        var groups = Regexes.Line().Match(line).Groups;
        return new Robot(new Position(groups[1].ToInt(), groups[2].ToInt()),
            new Velocity(groups[3].ToInt(), groups[4].ToInt()));
    });

int Mod(int num, int modulus) => (num % modulus + modulus) % modulus;


record Map(Robot[] Robots)
{
    // internal int Width => 11;
    // internal int Height => 7;

    internal int Width => 101;
    internal int Height => 103;

    int MidPointY => (Width - 1) / 2;
    int MidPointX => (Height - 1) / 2;

    internal void MoveRobots(int increment)
    {
        foreach (var robot in Robots)
        {
            robot.Move(this, increment);
            Console.WriteLine(robot.Position);
        }
    }

    internal int SafetyFactor
    {
        get
        {
            var counts = QuadrantCounts.ToArray();
            foreach (var count in counts) Console.WriteLine(count);
            return QuadrantCounts.Aggregate(1, (cur, val) => cur * val);
        }
    }

    IEnumerable<int> QuadrantCounts => Robots
        .Where(robot => robot.Position.X != MidPointX && robot.Position.Y != MidPointY)
        .GroupBy(robot => (robot.Position.X > MidPointX, robot.Position.Y > MidPointY))
        .Select(group => group.Count());
}


class Robot(Position position, Velocity velocity)
{
    internal Position Position { get; private set; } = position;
    internal Velocity Velocity { get; } = velocity;

    internal void Move(Map map, int increments)
    {
        var newX = Mod(Position.X + Velocity.X * increments, map.Width);
        var newY = Mod(Position.Y + Velocity.Y * increments, map.Height);
        Position = new Position(newX, newY);
    }

    int Mod(int num, int modulus) => (num % modulus + modulus) % modulus;
};

record struct Position(int X, int Y);

record struct Velocity(int X, int Y);

partial class Regexes
{
    [GeneratedRegex(@"p=(\d+),(\d+) v=(-?\d+),(-?\d+)")]
    internal static partial Regex Line();
}

static class Extensions
{
    internal static int ToInt(this Group group) => int.Parse(group.Value);
}