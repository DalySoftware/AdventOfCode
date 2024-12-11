// See https://aka.ms/new-console-template for more information

// var testInput = "125 17";

var input = "0 89741 316108 7641 756 9 7832357 91";

var stones = Parse(input);
// var finalStones = Blink(stones, 75);

var score = 0L;
foreach (var stone in stones) score += stone.Score();

Console.WriteLine("Score:");
Console.WriteLine(score);

// Console.WriteLine("Stones:");
// Console.WriteLine(finalStones.LongCount());

return;

// IEnumerable<long> Blink(IEnumerable<long> stones, int blinks)
// {
//     var result = stones;
//     while (blinks > 0)
//     {
//         result = result.SelectMany(Extensions.Transform);
//         blinks--;
//     }
//
//     return result;
// }

IEnumerable<Stone> Parse(string input)
    => input.Split(" ").Select(l => new Stone(long.Parse(l), 0));

static class Extensions
{
    static long[] Split(this long stone)
    {
        var divisor = (long)Math.Pow(10, stone.Digits() / 2);
        return [stone / divisor, stone % divisor];
    }

    static bool HasEvenDigits(this long stone)
        => stone.Digits() % 2 == 0;

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

    static readonly ILookup<long, long> TransformCache = new List<long>().ToLookup(l => l);

    internal static Span<Stone> Transform(Stone stone)
    {
        if (TransformCache.TryGetValue(stone.Value, out var cached))
            foreach (var cachedValue in cached)
                yield return new Stone(cachedValue, stone.Iteration + 1);

        if (stone.Value == 0)
        {
            var newStone = new Stone(1, stone.Iteration + 1);

            yield return ;
        }
        else if (stone.Value.HasEvenDigits())
        {
            foreach (var value in stone.Value.Split())
                yield return new Stone(value, stone.Iteration + 1);
        }
        else
        {
            yield return new Stone(stone.Value * 2024, stone.Iteration + 1);
        }
    }

    static readonly Dictionary<Stone, long> ScoreCache = new();

    internal static long Score(this Stone stone)
    {
        if (ScoreCache.TryGetValue(stone, out var cachedStoneScore)) return cachedStoneScore;

        var score = 0L;

        var toCalculate = new Stack<Stone>();
        toCalculate.Push(stone);

        while (toCalculate.TryPop(out var current))
        {
            if (current.Iteration >= MaxIterations) score += 1;

            foreach (var newStone in current.Transform()) toCalculate.Push(newStone);
        }

        ScoreCache[stone] = score;
        return score;
    }

    const int MaxIterations = 5;
}

// record struct Stone(ReadOnlySpan<byte> Value, int Iteration);

ref struct Stone(ReadOnlySpan<byte> Value, int Iteration);