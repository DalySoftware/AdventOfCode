// var testInput = "125 17";

var input = "0 89741 316108 7641 756 9 7832357 91";

var stones = Parse(input);
var finalStones = Blink(stones, 75);

var count = 0L;
foreach (var _ in finalStones)
{
    if (count % 10_000_000 == 0) Console.WriteLine(count);
    count++;
}

Console.WriteLine("Stones:");
Console.WriteLine(count);
return;

IEnumerable<string> Blink(IEnumerable<string> stones, int blinks)
{
    var result = stones;
    while (blinks > 0)
    {
        result = TransformStones(result);
        blinks--;
    }

    return result;
}

static IEnumerable<string> TransformStones(IEnumerable<string> stones)
{
    foreach (var stone in stones)
    foreach (var transformedStone in stone.Transform())
        yield return transformedStone;
}

static IEnumerable<string> Parse(string input) => input.Split(" ");

static class Extensions
{
    static readonly Dictionary<string, string[]> TransformCache = new();

    internal static IEnumerable<string> Transform(this string stone)
    {
        if (TransformCache.TryGetValue(stone, out var cached))
        {
            foreach (var str in cached) yield return str;

            yield break;
        }

        if (stone == "0")
        {
            TransformCache[stone] = ["1"];
            yield return "1";
            yield break;
        }

        if (stone.Length % 2 == 0)
        {
            var midPoint = stone.Length / 2;
            var a = stone[..midPoint];
            var b = stone[midPoint..].TruncateLeadingZeros();

            TransformCache[stone] = [a, b];
            yield return a;
            yield return b;
            yield break;
        }

        var result = (long.Parse(stone) * 2024).ToString();
        TransformCache[stone] = [result];
        yield return result;
    }

    static string TruncateLeadingZeros(this string input)
    {
        // Use TrimStart to remove leading zeroes, then ensure at least one character remains
        var truncated = input.TrimStart('0');

        return string.IsNullOrEmpty(truncated) ? "0" : truncated;
    }
}