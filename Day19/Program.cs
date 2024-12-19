// See https://aka.ms/new-console-template for more information

// var input = """
//             r, wr, b, g, bwu, rb, gb, br
//
//             brwrr
//             bggr
//             gbbr
//             rrbgbr
//             ubwu
//             bwurrg
//             brgr
//             bbrgwb
//             """;

var input = File.ReadAllText("input.txt");

var (designs, towels) = Solver.Parse(input);

var count = new Solver(designs, towels).NumberOfWays();

Console.WriteLine("Result:");
Console.WriteLine(count);

return;

class Solver(Design[] designs, Towel[] towels)
{
    internal long NumberOfWays() =>
        designs.Sum(d => CachedWaysToMakePattern(d.Pattern));


    readonly Dictionary<string, long> _waysCache = new();

    long CachedWaysToMakePattern(string pattern) => _waysCache.TryGetValue(pattern, out var cached)
        ? cached
        : _waysCache[pattern] = WaysToMakePattern(pattern);

    long WaysToMakePattern(string pattern)
    {
        // Console.WriteLine(pattern);
        if (pattern == string.Empty) return 1;

        var possibleStarts = towels.Where(t => CachedMatchesStart(t, pattern));
        return possibleStarts.Sum(p =>
            CachedWaysToMakePattern(pattern[p.Pattern.Length..]));
    }

    internal static (Design[] Designs, Towel[] Towels) Parse(string input)
    {
        var lines = input.Split(["\r\n", "\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var towels = lines[0].Split(",", StringSplitOptions.TrimEntries).Select(str => new Towel(str))
            .ToArray();

        var designs = lines[1..].Select(line => new Design(line)).ToArray();
        return (designs, towels);
    }


    static readonly Dictionary<(Towel, string), bool> MatchesCache = new();

    static bool CachedMatchesStart(Towel towel, string pattern) =>
        MatchesCache.TryGetValue((towel, pattern), out var cached)
            ? cached
            : MatchesCache[(towel, pattern)] = MatchesStart(towel, pattern);

    static bool MatchesStart(Towel towel, string pattern)
        => towel.Pattern.Length <= pattern.Length &&
           pattern[..towel.Pattern.Length] == towel.Pattern;
}

readonly record struct Design(string Pattern);

readonly record struct Towel(string Pattern);