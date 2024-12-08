// See https://aka.ms/new-console-template for more information

using Antennae = System.Collections.Generic.Dictionary<char, System.Collections.Generic.List<Vector2>>;
using Antinodes = System.Collections.Generic.Dictionary<char, System.Collections.Generic.List<Vector2>>;

// var input = """
//             ............
//             ........0...
//             .....0......
//             .......0....
//             ....0.......
//             ......A.....
//             ............
//             ............
//             ........A...
//             .........A..
//             ............
//             ............
//             """;
var input = File.ReadAllText("input.txt");

var inputLines = input.Split(["\n", "\r\n"], StringSplitOptions.RemoveEmptyEntries);

var antennae = Read(inputLines);
var antinodes = Antinodes(antennae, inputLines[0].Length - 1, inputLines.Length - 1);

var count = antinodes.SelectMany(a => a.Value).Distinct().Count();

Console.WriteLine("======  Result:  ======");
Console.WriteLine(count);

return;

Antennae Read(IEnumerable<string> lines)
{
    var chars = lines
        .SelectMany((line, lineIndex) =>
            line.ToCharArray().Select((ch, chIndex) => (ch, Position: new Vector2(lineIndex, chIndex))))
        .Where(x => x.ch != '.')
        .ToList();

    return chars
        .GroupBy(a => a.ch, a => a.Position)
        .ToDictionary(group => group.Key, group => group.ToList());
}

Antinodes Antinodes(Antennae antennae, int maxX, int maxY)
{
    return antennae.ToDictionary(kv => kv.Key, kv =>
    {
        var pairs = kv.Value
            .SelectMany(_ => kv.Value, (a, b) => (a, b))
            .Where(pair => pair.a != pair.b);

        var antinodes = pairs
            .SelectMany(x => x.a.Antinodes(x.b))
            .ToList();

        return antinodes
            .Where(a => a.X >= 0 && a.X <= maxX && a.Y >= 0 && a.Y <= maxY)
            .ToList();
    });
}

readonly record struct Vector2(int X, int Y)
{
    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);

    internal IEnumerable<Vector2> Antinodes(Vector2 other)
    {
        var direction = this - other;
        return [this + direction, other - direction];
    }
};