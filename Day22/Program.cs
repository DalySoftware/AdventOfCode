// See https://aka.ms/new-console-template for more information

using PriceCache = System.Collections.Generic.Dictionary<long[], long>;

// List<long> inputs =
// [
//     1,
//     2,
//     3,
//     2024,
// ];

var inputs = ParseInput();

var monkeys = inputs.Select(l => new Monkey(l)).ToArray();

foreach (var monkey in monkeys) monkey.NthNext(2000);

var mergedDictionary = monkeys.Select(m => m.PriceCache).Merge();
var bestSequence = mergedDictionary.MaxBy(kv => kv.Value).Key;
Console.WriteLine("Sequence: " + string.Join(',', bestSequence));

Console.WriteLine("Result:");
Console.WriteLine(mergedDictionary[bestSequence]);

return;

IEnumerable<long> ParseInput() =>
    File.ReadAllLines("input.txt")
        .Where(l => !string.IsNullOrWhiteSpace(l))
        .Select(long.Parse);


class Monkey(long initialSecret)
{
    internal PriceCache PriceCache { get; } = new(new DifferenceKeyComparer());

    internal long NthNext(int n) =>
        Enumerable.Range(0, n)
            .Aggregate(
                (Secret: initialSecret,
                    Differences: new FixedLengthQueue<long>([], 4)),
                (state, _) =>
                {
                    var (lastSecret, queue) = state;

                    var currentSecret = lastSecret.Next();
                    var currentPrice = currentSecret.LastDigit();

                    var difference = currentPrice - lastSecret.LastDigit();
                    queue.Enqueue(difference);
                    if (queue.Count == 4)
                        PriceCache.TryAdd(queue.ToArray(), currentPrice);

                    return (currentSecret, queue);
                }
            )
            .Secret;
}

class DifferenceKeyComparer : IEqualityComparer<long[]>
{
    public bool Equals(long[]? x, long[]? y)
    {
        if (x == null || y == null) return false;
        if (x.Length != y.Length) return false;

        return !x.Where((t, i) => t != y[i]).Any();
    }

    public int GetHashCode(long[] obj) =>
        obj.Aggregate(17, (current, value) => current * 31 + value.GetHashCode());
}


static class Extensions
{
    internal static long Next(this long secret)
    {
        secret = secret.Mix(secret * 64).Prune();
        secret = secret.Mix(secret / 32).Prune();
        return secret.Mix(secret * 2048).Prune();
    }

    static long Mix(this long secret, long other) => secret ^ other;

    static long Prune(this long secret) => secret % 16777216;

    internal static PriceCache Merge(this IEnumerable<PriceCache> dictionaries)
    {
        var result = new PriceCache(new DifferenceKeyComparer());

        foreach (var dict in dictionaries)
        foreach (var kvp in dict)
            if (result.TryGetValue(kvp.Key, out var existingValue))
                result[kvp.Key] = existingValue + kvp.Value;
            else
                result[kvp.Key] = kvp.Value;

        return result;
    }

    internal static int LastDigit(this long num) => (int)(num % 10);
}

class FixedLengthQueue<T>(IEnumerable<T> items, int maxSize) : Queue<T>(items)
{
    int MaxSize { get; } = maxSize;

    public new void Enqueue(T item)
    {
        base.Enqueue(item);
        while (Count > MaxSize) Dequeue();
    }
}