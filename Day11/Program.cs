// See https://aka.ms/new-console-template for more information

// var testInput = "125 17";

var input = "0 89741 316108 7641 756 9 7832357 91";

var stones = Parse(input);
var finalStones = Blink(stones, 25);

var count = 0L;
foreach (var stone in finalStones) count++;

Console.WriteLine("Stones:");
Console.WriteLine(count);
// Console.WriteLine(finalStones.LongCount());

return;

IEnumerable<string> Blink(IEnumerable<string> stones, int blinks)
{
    var result = stones;
    while (blinks > 0)
    {
        result = result.SelectMany(Extensions.Transform);
        blinks--;
    }

    return result;
}

IEnumerable<string> Parse(string input) => input.Split(" ");

static class Extensions
{
    static double Digits(this long stone)
    {
        if (stone == 0) return 1;

        var digits = 0;

        while (stone != 0)
        {
            stone /= 10;
            digits++;
        }

        return digits;
        // return Math.Floor(Math.Log10(stone) + 1);
    }

    static readonly Dictionary<string, string[]> TransformCache = new();

    internal static IEnumerable<string> Transform(this string stone)
    {
        if (TransformCache.TryGetValue(stone, out var cached))
            foreach (var str in cached)
                yield return str;

        if (stone == "0")
        {
            TransformCache[stone] = ["1"];
            yield return "1";
        }
        else if (stone.Length % 2 == 0)
        {
            var midPoint = stone.Length / 2;
            var a = stone[..midPoint];
            var b = stone[midPoint..];

            TransformCache[stone] = [a, b];
            yield return a;
            yield return b;
        }
        else
        {
            var result = (long.Parse(stone) * 2024).ToString();
            TransformCache[stone] = [result];
            yield return result;
        }
    }
}