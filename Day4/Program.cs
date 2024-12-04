// See https://aka.ms/new-console-template for more information

// var testInput = "MMMSXXMASM\nMSAMXMSMSA\nAMXSXMAAMM\nMSAMASMSMX\nXMASAMXAMM\nXXAMMXXAMA\nSMSMSASXSS\nSAXAMASAAA\nMAMMMXMMMM\nMXMXAXMASX";
var input = File.ReadAllText("input.txt");

var searchMatrix = ConvertToMatrix(input);

Part1.FindWords(searchMatrix);

return;

char[][] ConvertToMatrix(string input) =>
    input.Split(["\n", "\r\n"], StringSplitOptions.None)
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .Select(line => line.ToCharArray())
        .ToArray();

internal static class Part1
{
    internal static void FindWords(char[][] matrix)
    {
        List<Direction> directions = [
            new(-1, -1),
            new(-1, 0),
            new(-1, 1),
            new(0, -1),
            new(0, 1),
            new(1, -1),
            new(1, 0),
            new(1, 1),
        ];

        var cells = Enumerable.Range(0, matrix.Length)
            .SelectMany(_ => Enumerable.Range(0, matrix[0].Length), (x, y) => new Address(x, y))
            .ToList();
        
        var searches = directions.SelectMany(_ => cells, (direction, address) => new DirectionalSearch(direction, address));
        
        var found = searches.Count(s => TryFindWord(s, matrix));
        Console.WriteLine($"Found {found} matches");
    }

    private static bool TryFindWord(DirectionalSearch search, char[][] matrix)
    {
        var currentAddress = search.Address;
        
        var maxX = matrix.Length - 1;
        var maxY = matrix[0].Length - 1;
        
        const string targetWord = "XMAS";
        foreach (var targetChar in targetWord)
        {
            if (!currentAddress.WithinBounds(maxX, maxY)
                || matrix[currentAddress.X][currentAddress.Y] != targetChar)
            {
                return false;
            }

            currentAddress = currentAddress.Move(search.Direction);
        }
        
        return true;
    }

    private record DirectionalSearch(Direction Direction, Address Address);
}

internal record Direction(int XIncrement, int YIncrement);
internal record Address(int X, int Y)
{
    internal bool WithinBounds(int maxX, int maxY) =>
        X >= 0 && X <= maxX && Y >= 0 && Y <= maxY;

    internal bool TouchesBounds(int maxX, int maxY) =>
        X == 0 || X == maxX || Y == 0 || Y == maxY;

    internal Address Move(Direction direction) => new(X + direction.XIncrement, Y + direction.YIncrement);
}

