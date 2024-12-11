// See https://aka.ms/new-console-template for more information

// var testInput = "125 17";

using System.Collections;

var input = "0 89741 316108 7641 756 9 7832357 91";

var stones = Parse(input);
// var finalStones = Blink(stones, 75);

var score = stones.Sum(Extensions.Score);

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

    static readonly Dictionary<long, Stone[]> TransformCache = new();

    internal static IEnumerable<Stone> Transform(this Stone stone)
    {
        if (TransformCache.TryGetValue(stone.Value, out var cached))
            foreach (var cachedStone in cached.Select(s => s with { Iteration = stone.Iteration + 1 }))
                yield return cachedStone;

        if (stone.Value == 0)
            yield return new Stone(1, stone.Iteration + 1);
        else if (stone.Value.HasEvenDigits())
            foreach (var newStone in stone.Value.Split().Select(v => new Stone(v, stone.Iteration + 1)))
                yield return newStone;
        else yield return new Stone(stone.Value * 2024, stone.Iteration + 1);
    }

    static readonly Dictionary<Stone, long> ScoreCache = new();

    internal static long Score(Stone stone)
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

    const int MaxIterations = 25;
}

record struct Stone(long Value, int Iteration);