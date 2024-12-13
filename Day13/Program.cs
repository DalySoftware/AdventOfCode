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

var xStrategies = Extensions.CandidateStrategies(26, 67, 10000000012748);
foreach (var xStrategy in xStrategies) Console.WriteLine(xStrategy);

// var machines = input.Parse();
// var cost = machines.Sum(Extensions.Cost);
//
// Console.WriteLine("Cost:");
// Console.WriteLine(cost);

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

    internal static IEnumerable<Strategy> CandidateStrategies(long aIncrement, long bIncrement, long target)
    {
        var (g, x, y) = ExtendedGcd(aIncrement, bIncrement);
        if (target % g != 0) yield break; // No solution if target is not a multiple of gcd

        var scale = target / g;
        var baseX = x * scale;
        var baseY = y * scale;

        var kStart = (long)Math.Ceiling((double)(-baseX * g) / bIncrement);
        var kEnd = (long)Math.Floor((double)(baseY * g) / aIncrement);

        Console.WriteLine(kStart + " | " + kEnd);

        for (var k = kStart; k <= kEnd; k++)
        {
            var xk = baseX + k * (bIncrement / g);
            var yk = baseY - k * (aIncrement / g);

            Console.WriteLine(xk + " | " + yk);

            if (xk > 0 && yk > 0) yield return new Strategy(xk, yk);
        }
    }

    static (long gcd, long x, long y) ExtendedGcd(long a, long b)
    {
        long x = 1, y = 0;
        long xLast = 0, yLast = 1;
        long temp, q, r;

        while (b != 0)
        {
            q = a / b;
            r = a % b;

            temp = x;
            x = xLast - q * x;
            xLast = temp;

            temp = y;
            y = yLast - q * y;
            yLast = temp;

            a = b;
            b = r;
        }

        return (a, xLast, yLast);
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