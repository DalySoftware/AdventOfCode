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

var (designs, towels) = Parse(input);

var designCount = PossibleDesigns(designs, towels);

Console.WriteLine("Result:");
Console.WriteLine(designCount);

return;

int PossibleDesigns(Design[] designs, Towel[] towels) => designs.Count(d => IsPossible(d.Pattern, towels));

bool IsPossible(string pattern, Towel[] towels) => WaysToMakePattern(pattern, towels).Any();

IEnumerable<Towel[]> WaysToMakePattern(string pattern, Towel[] towels)
{
    if (pattern == string.Empty) return [towels];

    var possibleStarts = towels.Where(t => t.MatchesStart(pattern));
    return possibleStarts.SelectMany(p => WaysToMakePattern(pattern[p.Pattern.Length..], [..towels, p]));
}

(Design[] Designs, Towel[] Towels) Parse(string input)
{
    var lines = input.Split(["\r\n", "\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    var towels = lines[0].Split(",", StringSplitOptions.TrimEntries).Select(str => new Towel(str))
        .ToArray();

    var designs = lines[1..].Select(line => new Design(line)).ToArray();
    return (designs, towels);
}

record Design(string Pattern);

record Towel(string Pattern)
{
    internal bool MatchesStart(string pattern) =>
        Pattern.Length <= pattern.Length &&
        Pattern.Select((color, index) => (color, index)).All(x => pattern[x.index] == x.color);
};