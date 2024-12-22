// See https://aka.ms/new-console-template for more information

// List<long> inputs =
// [
//     1,
//     10,
//     100,
//     2024,
// ];

var inputs = ParseInput();

// foreach (var input in inputs) Console.WriteLine(input + " : " + input.NthNext(2000));

var result = inputs.Sum(i => i.NthNext(2000));
Console.WriteLine("Result:");
Console.WriteLine(result);

return;

IEnumerable<long> ParseInput() =>
    File.ReadAllLines("input.txt")
        .Where(l => !string.IsNullOrWhiteSpace(l))
        .Select(long.Parse);


static class Extensions
{
    internal static long NthNext(this long secret, int n) => ApplyNTimes(secret, Next, n);

    static long Next(this long secret)
    {
        secret = secret.Mix(secret * 64).Prune();
        secret = secret.Mix(secret / 32).Prune();
        return secret.Mix(secret * 2048).Prune();
    }

    static T ApplyNTimes<T>(T initial, Func<T, T> func, int n) =>
        Enumerable.Range(0, n).Aggregate(initial, (current, _) => func(current));

    static long Mix(this long secret, long other) => secret ^ other;

    static long Prune(this long secret) => secret % 16777216;
}