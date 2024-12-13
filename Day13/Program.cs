// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;

var input = """
            Button A: X+94, Y+34
            Button B: X+22, Y+67
            Prize: X=8400, Y=5400

            Button A: X+26, Y+66
            Button B: X+67, Y+21
            Prize: X=12748, Y=12176

            Button A: X+17, Y+86
            Button B: X+84, Y+37
            Prize: X=7870, Y=6450

            Button A: X+69, Y+23
            Button B: X+27, Y+71
            Prize: X=18641, Y=10279
            """;

// var input = File.ReadAllText("input.txt");

var machines = input.Parse();
var cost = machines.Sum(Extensions.Cost);

Console.WriteLine("Cost:");
Console.WriteLine(cost);

return;


static class Extensions
{
    internal static long Cost(Machine machine) => machine.OptimalStrategy()?.Cost() ?? 0;

    static Strategy? OptimalStrategy(this Machine machine)
    {
        var xStrategies = CandidateStrategies(machine.A.XIncrement, machine.B.XIncrement, machine.Prize.X);
        var yStrategies = CandidateStrategies(machine.A.YIncrement, machine.B.YIncrement, machine.Prize.Y);

        // Hit both the X and Y coordinate
        var validStrategies =
            xStrategies.Where(x => yStrategies.Any(y => x.APresses == y.APresses && x.BPresses == y.BPresses));
        return validStrategies.MinBy(Cost);
    }

    static long Gcd(long a, long b, out long x, out long y)
    {
        if (b == 0)
        {
            x = 1;
            y = 0;
            return a;
        }

        var gcd = Gcd(b, a % b, out x, out y);
        var temp = y;
        y = x - a / b * y;
        x = temp;

        return gcd;
    }

    static IEnumerable<Strategy> CandidateStrategies(long aIncrement, long bIncrement, long target)
    {
        var gcd = Gcd(aIncrement, bIncrement, out var a0, out var b0);

        if (target % gcd != 0) yield break; // No solutions if T is not divisible by GCD

        // Scale the solution to satisfy the equation X * A + Y * B = T
        a0 *= target / gcd;
        b0 *= target / gcd;

        Console.WriteLine(a0);
        Console.WriteLine(b0);

        // Generate all positive solutions
        var k = 0;
        while (true)
        {
            var aPresses = a0 + k * (bIncrement / gcd);
            var bPresses = b0 - k * (aIncrement / gcd);

            Console.WriteLine(aPresses + " | " + bPresses);

            if (aPresses > 0 && bPresses > 0)
                yield return new Strategy(aPresses, bPresses);
            else if (aPresses <= 0 || bPresses <= 0) break;

            k++;
        }
    }

    // static IEnumerable<Strategy> CandidateStrategies(long aIncrement, long bIncrement, long target)
    // {
    //     var gcd = GreatestCommonDivisor(aIncrement, bIncrement);
    //
    //     // Check if T is divisible by the GCD of X and Y
    //     if (target % gcd != 0) yield break; // No solutions
    //
    //     // Reduce the equation by GCD
    //     aIncrement /= gcd;
    //     bIncrement /= gcd;
    //     target /= gcd;
    //
    //     // Iterate to find all valid combinations
    //     for (var aPresses = 0L; aPresses * aIncrement <= target && aPresses >= 0; aPresses++)
    //     {
    //         if (aPresses % 1_000_000 == 0) Console.WriteLine(aPresses);
    //         var remainder = target - aIncrement * aPresses;
    //         if (remainder % bIncrement != 0) continue;
    //
    //         var bPresses = remainder / bIncrement;
    //         yield return new Strategy(aPresses, bPresses);
    //     }
    // }


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
        var x = long.Parse(groups[1].Value) + 10000000000000;
        var y = long.Parse(groups[2].Value) + 10000000000000;
        return new Prize(x, y);
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

record Button(long XIncrement, long YIncrement);

record Prize(long X, long Y);

record Strategy(long APresses, long BPresses);