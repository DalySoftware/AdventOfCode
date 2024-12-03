// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;

// var input = "xmul(2,4)%&mul[3,7]!@^do_not_mul(5,5)\r\n+mul(32,64]then(mul(11,8)mul(8,5))";
// var input = "xmul(2,4)&mul[3,7]!^don't()_mul(5,5)+mul(32,64](mul(11,8)undo()?mul(8,5))";
var input = File.ReadAllText("input.txt");

Console.WriteLine("Total:");
Console.WriteLine(CalculateTotal(input));
return;

int CalculateTotal(string input) => MulRegex()
    .Matches(RemoveDontSections(input))
    .Sum(m => int.Parse(m.Groups[1].Value) * int.Parse(m.Groups[2].Value));

string RemoveDontSections(string input)
{
    var removeMatchedDonts = MatchedDontRegex().Replace(input, "");
    return UnmatchedDontRegex().Replace(removeMatchedDonts, "");
}

partial class Program
{
    [GeneratedRegex(@"mul\((\d+),(\d+)\)")]
    private static partial Regex MulRegex();

    [GeneratedRegex(@"don't\(\).*?(?:do\(\))", RegexOptions.Singleline)]
    private static partial Regex MatchedDontRegex();

    [GeneratedRegex(@"don't\(\).*", RegexOptions.Singleline)]
    private static partial Regex UnmatchedDontRegex();
}