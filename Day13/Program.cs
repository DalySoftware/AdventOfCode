// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using Day13;

Tests.Run();

// var input = """
//
//
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

// var xStrategies = Extensions.CandidateStrategies(26, 67, 10000000012748);
// foreach (var xStrategy in xStrategies) Console.WriteLine(xStrategy);

var machines = input.Parse();
var cost = machines.Sum(Extensions.Cost);

Console.WriteLine("Cost:");
Console.WriteLine(cost);

return;


static class Extensions
{
    internal static long Cost(Machine machine) => machine.OptimalStrategy()?.Cost() ?? 0;

    static Strategy? OptimalStrategy(this Machine machine) =>
        // var xGcd = GCD(machine.A.XIncrement, machine.B.XIncrement);
        // var xDivisible = machine.Prize.X % xGcd == 0;
        // var yGcd = GCD(machine.A.YIncrement, machine.B.YIncrement);
        // var yDivisible = machine.Prize.Y % yGcd == 0;
        //
        // if (!xDivisible || !yDivisible) return null;
        // var strategies = CandidateStrategies(machine.A.XIncrement, machine.B.YIncrement, machine.Prize.X, strategy =>
        //     strategy.APresses * machine.A.YIncrement + strategy.BPresses * machine.B.YIncrement == machine.Prize.Y);
        // return strategies.DefaultIfEmpty().MinBy(Cost);
        SolveDiophantine(machine);

    // Extended Euclidean Algorithm to find gcd and the coefficients
    static (long, long, long) ExtendedGCD(long a, long b)
    {
        if (b == 0) return (a, 1, 0);

        var (gcd, x1, y1) = ExtendedGCD(b, a % b);
        return (gcd, y1, x1 - a / b * y1);
    }

    // Method to solve the Diophantine system and minimize the cost function
    public static Strategy? SolveDiophantine(Machine machine)
    {
        // Step 1: Solve the first equation for BPresses in terms of APresses
        var minCost = long.MaxValue;
        Strategy? strategy = null;

        // Iterate over possible values of APresses
        for (long aPresses = 0; aPresses <= machine.Prize.X / machine.A.XIncrement; aPresses++)
        {
            // Calculate BPresses from the first equation
            var remainingX = machine.Prize.X - aPresses * machine.A.XIncrement;
            if (remainingX % machine.B.XIncrement != 0) continue;

            var bPresses = remainingX / machine.B.XIncrement;

            // Check if the second equation is satisfied
            if (aPresses * machine.A.YIncrement + bPresses * machine.B.YIncrement != machine.Prize.Y) continue;

            // Calculate the cost
            var candidateStrategy = new Strategy(aPresses, bPresses);
            var cost = candidateStrategy.Cost();

            // Minimize the cost
            if (cost < minCost && aPresses + bPresses > 0)
            {
                Console.WriteLine("New minimum: " + strategy);
                strategy = candidateStrategy;
                minCost = cost;
            }
        }

        if (strategy == null) Console.WriteLine("No solution");
        else Console.WriteLine("Optimal: " + strategy);
        return strategy;
    }

    static long Cost(this Strategy strategy) => Cost(strategy.APresses, strategy.BPresses);
    static long Cost(long APresses, long BPresses) => 3 * APresses + 1 * BPresses;


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
            })
            .ToArray();
    }

    static Button ParseButton(this string line)
    {
        var groups = Regexes.Button().Match(line).Groups;
        return new Button(int.Parse(groups[1].Value), int.Parse(groups[2].Value));
    }

    static Prize ParsePrize(this string line)
    {
        var groups = Regexes.Prize().Match(line).Groups;
        var x = long.Parse(groups[1].Value) + 10000000000000;
        var y = long.Parse(groups[2].Value) + 10000000000000;
        var prize = new Prize(x, y);
        Console.WriteLine(prize);
        return prize;
    }
}

partial class Regexes
{
    [GeneratedRegex(@"Button [AB]: X\+(\d+), Y\+(\d+)")]
    internal static partial Regex Button();

    [GeneratedRegex(@"Prize: X=(\d+), Y=(\d+)")]
    internal static partial Regex Prize();
}


record struct Machine(Button A, Button B, Prize Prize);

record struct Button(long XIncrement, long YIncrement);

record struct Prize(long X, long Y);

record struct Strategy(long APresses, long BPresses);