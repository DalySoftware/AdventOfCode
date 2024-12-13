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

    // Method to find the particular solution for the Diophantine equations
    public static Strategy? SolveDiophantine(Machine machine)
    {
        // Step 1: Find the gcd of the coefficients
        long gcd1, x1, y1;
        (gcd1, x1, y1) = ExtendedGCD(machine.A.XIncrement, machine.B.XIncrement);

        // Check if the gcd divides X
        if (machine.Prize.X % gcd1 != 0)
        {
            Console.WriteLine("No solution exists for X.");
            return null;
        }

        var scale = machine.Prize.X / gcd1;
        x1 *= scale;
        y1 *= scale;

        var (gcd2, _, _) = ExtendedGCD(machine.A.YIncrement, machine.B.YIncrement);

        // Check if the gcd divides Y
        if (machine.Prize.Y % gcd2 != 0)
        {
            Console.WriteLine("No solution exists for Y.");
            return null;
        }

        // Now x1 and y1 provide one solution for the first equation
        // and x2 and y2 provide a solution for the second equation

        // Combine the two results into one system using the method for solving linear Diophantine systems
        // We need to find integers p and q such that we satisfy both equations.

        // Now that we have the general solution, minimize the cost function
        var minCost = long.MaxValue;
        long optimalAPresses = -1;
        long optimalBPresses = -1;

        // We loop over a reasonable range to adjust the general solution
        // Since this is still a large scale problem, we must minimize directly.
        for (long p = -100000; p <= 100000; p++) // Adjust the loop range as needed
        {
            var aPresses = x1 + p * machine.B.XIncrement;
            var bPresses = y1 - p * machine.A.XIncrement;

            if (aPresses < 0 || bPresses < 0 || aPresses + bPresses <= 0) continue;

            var cost = 3 * aPresses + bPresses;
            if (cost < minCost)
            {
                minCost = cost;
                optimalAPresses = aPresses;
                optimalBPresses = bPresses;
            }
        }

        if (optimalAPresses != -1 && optimalBPresses != -1)
        {
            var strategy = new Strategy(optimalAPresses, optimalBPresses);
            Console.WriteLine("Found optimal: " + strategy);
            return strategy;
        }

        Console.WriteLine("No valid solution found.");
        return null;
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
        var x = long.Parse(groups[1].Value); //+ 10000000000000;
        var y = long.Parse(groups[2].Value); //+ 10000000000000;
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