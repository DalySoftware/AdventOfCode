// var testInput = "125 17";

using System.Buffers;

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
    // Estimate capacity
    var initialCapacity = (int)Math.Abs(Math.Pow(2, stones.Length));

    // Rent an array from the pool
    var pool = ArrayPool<string>.Shared;
    var rentedArray = pool.Rent(initialCapacity);
    var count = 0;

    try
    {
        foreach (var stone in stones)
        foreach (var transformedStone in stone.Transform())
        {
            if (count >= rentedArray.Length)
            {
                // Double the size of the array if the capacity is exceeded
                var newArray = pool.Rent(rentedArray.Length * 2);
                rentedArray.CopyTo(newArray, 0);
                pool.Return(rentedArray, true);
                rentedArray = newArray;
            }

            rentedArray[count++] = transformedStone;
        }

        // Return only the used portion as a ReadOnlySpan
        return new ReadOnlySpan<string>(rentedArray, 0, count);
    }
    finally
    {
        // Always return the rented array to the pool
        pool.Return(rentedArray, true);
    }
}

static ReadOnlySpan<string> Parse(string input) => input.Split(" ");

static class Extensions
{
    static readonly Dictionary<string, ReadOnlyMemory<string>> TransformCache =
        new();

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