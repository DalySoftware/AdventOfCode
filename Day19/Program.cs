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

var count = Solver.NumberOfWays(designs, towels);

Console.WriteLine("Result:");
Console.WriteLine(count);

return;

static class Solver
{
    internal static int NumberOfWays(Design[] designs, Towel[] towels) =>
        designs.Sum(d => WaysPossible(d.Pattern, towels));

    static int WaysPossible(string pattern, Towel[] towels) =>
        WaysToMakePattern(pattern, towels, Enumerable.Empty<Towel>());

    static int WaysToMakePattern(string pattern, Towel[] towelsAvailable, IEnumerable<Towel> towelsUsed)
    {
        if (pattern == string.Empty) return towelsUsed.Distinct().Count();

        var possibleStarts = towelsAvailable.Where(t => CachedMatchesStart(t, pattern));
        return possibleStarts.Sum(p =>
            WaysToMakePattern(pattern[p.Pattern.Length..], towelsAvailable, towelsUsed.Append(p)));
    }

    internal static (Design[] Designs, Towel[] Towels) Parse(string input)
    {
        var lines = input.Split(["\r\n", "\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var towels = lines[0].Split(",", StringSplitOptions.TrimEntries).Select(str => new Towel(str))
            .ToArray();

        var designs = lines[1..].Select(line => new Design(line)).ToArray();
        return (designs, towels);
    }

    internal readonly record struct Design(string Pattern);

    internal readonly record struct Towel(string Pattern);

    static readonly Dictionary<(Towel, string), bool> MatchesCache = new();

    static bool CachedMatchesStart(Towel towel, string pattern) =>
        MatchesCache.TryGetValue((towel, pattern), out var cached)
            ? cached
            : MatchesCache[(towel, pattern)] = MatchesStart(towel, pattern);

    static bool MatchesStart(Towel towel, string pattern)
        => towel.Pattern.Length <= pattern.Length &&
           pattern[..towel.Pattern.Length] == towel.Pattern;
}