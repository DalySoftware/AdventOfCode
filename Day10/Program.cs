// See https://aka.ms/new-console-template for more information

// var input = """
//             89010123
//             78121874
//             87430965
//             96549874
//             45678903
//             32019012
//             01329801
//             10456732 
//             """;

var input = File.ReadAllText("input.txt");

var map = Parse(input);

Console.WriteLine("Score:");
Console.WriteLine(map.Score());

return;

Map Parse(string input)
{
    var lines = input.Split(["\r\n", "\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    var tiles = lines
        .Select((line, yIndex) => (line, yIndex))
        .SelectMany(x => x.line.Select((ch, xIndex) => new Tile(ch.AsInt(), xIndex, x.yIndex)))
        .ToList();

    return new Map(tiles);
}

record Tile(int Value, int X, int Y)
{
    internal bool Neighbours(Tile other) =>
        (other.X == X && Math.Abs(other.Y - Y) == 1)
        || (other.Y == Y && Math.Abs(other.X - X) == 1);
}

record Map(List<Tile> Tiles);

static class Extensions
{
    internal static int AsInt(this char ch) => int.Parse(ch.ToString());

    internal static int Score(this Map map)
    {
        var nines = map.Tiles.Where(t => t.Value == 9);
        return nines.Sum(n => n.ReachableZeroes(map).Count);
    }

    static HashSet<Tile> ReachableZeroes(this Tile tile, Map map)
    {
        var neighbours = tile.DecreasingNeighbours(map).ToList();

        // Console.WriteLine(tile.Value + " at (" + tile.X + "," + tile.Y + ") : " + neighbours.Count);

        if (tile.Value == 0) return [tile];
        if (neighbours.Count == 0) return [];

        return neighbours.SelectMany(n => n.ReachableZeroes(map)).ToHashSet();
    }

    static IEnumerable<Tile> DecreasingNeighbours(this Tile tile, Map map)
        => map.Tiles.Where(t => t.Value == tile.Value - 1 && t.Neighbours(tile));
}