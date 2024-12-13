// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;

// var input = """
//             Button A: X+94, Y+34
//             Button B: X+22, Y+67
//             Prize: X=8400, Y=5400
//
//             Button A: X+26, Y+66
//             Button B: X+67, Y+21
//             Prize: X=12748, Y=12176
//
//             Button A: X+17, Y+86
//             Button B: X+84, Y+37
//             Prize: X=7870, Y=6450
//
//             Button A: X+69, Y+23
//             Button B: X+27, Y+71
//             Prize: X=18641, Y=10279
//             """;

var input = File.ReadAllText("input.txt");

var machines = input.Parse();
var cost = machines.Sum(Extensions.Cost);

Console.WriteLine("Cost:");
Console.WriteLine(cost);

return;


static class Extensions
{
    internal static int Cost(Machine machine) => machine.OptimalStrategy()?.Cost() ?? 0;

    static Strategy? OptimalStrategy(this Machine machine) => machine.CandidateStrategies().FirstOrDefault();

    // Lowest cost first
    static IEnumerable<Strategy> CandidateStrategies(this Machine machine)
    {
        var aPresses = 0;
        while (aPresses <= 100)
        {
            var bPresses = 0;
            var x = 0;
            var y = 0;
            while (x < machine.Prize.X && y < machine.Prize.Y && bPresses <= 100)
            {
                x = aPresses * machine.A.XIncrement + bPresses * machine.B.XIncrement;
                y = aPresses * machine.A.YIncrement + bPresses * machine.B.YIncrement;

                if (x == machine.Prize.X && y == machine.Prize.Y) yield return new Strategy(aPresses, bPresses);

                bPresses++;
            }


            aPresses += 1;
        }
    }

    static int Cost(this Strategy strategy) => Cost(strategy.APresses, strategy.BPresses);
    static int Cost(int APresses, int BPresses) => 3 * APresses + 1 * BPresses;


    internal static Machine[] Parse(this string input)
    {
        return input.Split(["\n", "\r\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select((value, index) => new { Index = index, Value = value })
            .GroupBy(x => x.Index / 3)
            .Select(x =>
            {
                var lines = x.ToArray();
                return new Machine(lines[0].Value.ParseButton(), lines[1].Value.ParseButton(),
                    lines[2].Value.ParsePrize());
            }).ToArray();
    }

    static Button ParseButton(this string line)
    {
        var groups = Regexes.Button().Match(line).Groups;
        return new Button(int.Parse(groups[1].Value), int.Parse(groups[2].Value));
    }

    static Prize ParsePrize(this string line)
    {
        var groups = Regexes.Prize().Match(line).Groups;
        return new Prize(int.Parse(groups[1].Value), int.Parse(groups[2].Value));
    }
}

partial class Regexes
{
    [GeneratedRegex(@"Button [AB]: X\+(\d+), Y\+(\d+)")]
    internal static partial Regex Button();

    [GeneratedRegex(@"Prize: X=(\d+), Y=(\d+)")]
    internal static partial Regex Prize();
}


record Machine(Button A, Button B, Prize Prize);

record Button(int XIncrement, int YIncrement);

record Prize(int X, int Y);

record Strategy(int APresses, int BPresses);