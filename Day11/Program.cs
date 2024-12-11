// var testInput = "125 17";

var input = "0 89741 316108 7641 756 9 7832357 91";

var stones = Parse(input);
var score = Blink(stones, 45);

Console.WriteLine("Stones:");
Console.WriteLine(score);
return;

long Blink(ReadOnlySpan<string> stones, int blinks)
{
    var currentStones = stones;
    while (blinks > 0)
    {
        Console.WriteLine("Blinks " + blinks);
        currentStones = TransformStones(currentStones);
        blinks--;
    }

    return currentStones.Length;
}

static ReadOnlySpan<string> TransformStones(ReadOnlySpan<string> stones)
{
    var newStones = new List<string>();
    foreach (var stone in stones)
    foreach (var transformedStone in stone.Transform())
        newStones.Add(transformedStone);

    return new ReadOnlySpan<string>(newStones.ToArray());
}

static ReadOnlySpan<string> Parse(string input) => input.Split(" ");

static class Extensions
{
    static readonly Dictionary<string, ReadOnlyMemory<string>> TransformCache = new();

    internal static ReadOnlySpan<string> Transform(this string stone)
    {
        if (TransformCache.TryGetValue(stone, out var cached)) return cached.Span;

        if (stone == "0")
        {
            string[] result = ["1"];
            TransformCache[stone] = result;
            return result;
        }

        if (stone.Length % 2 == 0)
        {
            var midPoint = stone.Length / 2;
            var a = stone[..midPoint];
            var b = stone[midPoint..].TruncateLeadingZeros();

            string[] result = [a, b];
            TransformCache[stone] = result;
            return result;
        }

        var transformed = (long.Parse(stone) * 2024).ToString();
        string[] finalResult = [transformed];
        TransformCache[stone] = finalResult;
        return finalResult;
    }

    static string TruncateLeadingZeros(this string input)
    {
        // Use TrimStart to remove leading zeroes, then ensure at least one character remains
        var truncated = input.TrimStart('0');

        return string.IsNullOrEmpty(truncated) ? "0" : truncated;
    }
}