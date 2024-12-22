// See https://aka.ms/new-console-template for more information

List<long> inputs =
[
    1,
    10,
    100,
    2024,
];

// var inputs = ParseInput();
var monkeys = inputs.Select(l => new Monkey(l)).ToArray();

foreach (var monkey in monkeys) Console.WriteLine(monkey.NthNext(2000));

var mergedDictionary = monkeys.Select(m => m.PriceCache).Merge();
var bestSequence = mergedDictionary.MaxBy(kv => kv.Value).Key;


// var result = monkeys.Sum(i => i.NthNext(2000));
Console.WriteLine("Result:");
Console.WriteLine(string.Join(',', bestSequence));

return;

IEnumerable<long> ParseInput() =>
    File.ReadAllLines("input.txt")
        .Where(l => !string.IsNullOrWhiteSpace(l))
        .Select(long.Parse);


class Monkey(long initialSecret)
{
    internal Dictionary<long[], long> PriceCache { get; } = new(new DifferenceKeyComparer());

    internal long NthNext(int n) =>
        Enumerable.Range(0, n)
            .Aggregate(
                new FixedLengthQueue<long>([initialSecret], 4), // Start with a queue containing the initial value
                (queue, accum) =>
                {
                    var nextValue = queue.Last().Next();
                    if (queue.Count == 4) PriceCache[queue.ToArray()] = nextValue;

                    var difference = nextValue - queue.Last();
                    return queue.Enqueue(difference);
                }
            )
            .Last();
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

    internal static Dictionary<long[], long> Merge(this IEnumerable<Dictionary<long[], long>> dictionaries)
    {
        var result = new Dictionary<long[], long>(new DifferenceKeyComparer());

        foreach (var dict in dictionaries)
        foreach (var kvp in dict)
            if (result.TryGetValue(kvp.Key, out var existingValue))
                result[kvp.Key] = existingValue + kvp.Value;
            else
                result[kvp.Key] = kvp.Value;

        return result;
    }
}

class FixedLengthQueue<T>(IEnumerable<T> items, int maxSize) : Queue<T>(items)
{
    int MaxSize { get; } = maxSize;

    public new FixedLengthQueue<T> Enqueue(T item)
    {
        base.Enqueue(item);
        while (Count > MaxSize) Dequeue();
        return this;
    }
}