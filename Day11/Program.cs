// var testInput = "125 17";

var input = "0 89741 316108 7641 756 9 7832357 91";

var stones = Parse(input);
var score = Blink(stones, 75);

Console.WriteLine("Stones:");
Console.WriteLine(score);
return;

long Blink(long[] stones, int blinks) => stones.CountStones(blinks);

static long[] Parse(string input) => input.Split(" ").Select(long.Parse).ToArray();

static class Extensions
{
    static readonly Dictionary<(long stone, int iteration), long> CountCache = new();

    internal static long CountStones(this long[] stones, int iteration)
        => stones.Sum(stone => stone.CachedCount(iteration));

    internal static long CachedCount(this long stone, int iteration) =>
        CountCache.TryGetValue((stone, iteration), out var cached)
            ? cached
            : CountCache[(stone, iteration)] = stone.CountStone(iteration);

    static long CountStone(this long stone, int iteration)
        => iteration == 0 ? 1 : CachedTransform(stone).CountStones(iteration - 1);


    static readonly Dictionary<long, long[]> TransformCache = new();

    internal static long[] CachedTransform(this long stone)
        => TransformCache.TryGetValue(stone, out var cached) ? cached : TransformCache[stone] = Transform(stone);

    internal static long[] Transform(this long stone)
    {
        if (stone == 0) return [1];

        var digits = stone.Digits();
        if (digits % 2 == 0)
        {
            var halves = stone.Split(digits);
            return [halves[0], halves[1]];
        }

        return [stone * 2024];
    }

    static long Digits(this long stone)
    {
        if (stone == 0) return 1;

        var digits = 0;
        while (stone != 0)
        {
            stone /= 10;
            digits++;
        }

        return digits;
    }

    static long[] Split(this long stone, long digits)
    {
        var divisor = (long)Math.Pow(10, digits / 2);
        return [stone / divisor, stone % divisor];
    }
}