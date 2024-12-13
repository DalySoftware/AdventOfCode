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

    static Strategy? OptimalStrategy(this Machine machine)
    {
        var xGcd = GCD(machine.A.XIncrement, machine.B.XIncrement);
        var xDivisible = machine.Prize.X % xGcd == 0;
        var yGcd = GCD(machine.A.YIncrement, machine.B.YIncrement);
        var yDivisible = machine.Prize.Y % yGcd == 0;

        if (!xDivisible || !yDivisible) return null;

        var xStrategies = CandidateStrategies(machine.A.XIncrement, machine.B.XIncrement, machine.Prize.X, strategy =>
            strategy.APresses * machine.A.YIncrement + strategy.BPresses * machine.B.YIncrement == machine.Prize.Y);
        // var yStrategies = CandidateStrategies(machine.A.YIncrement, machine.B.YIncrement, machine.Prize.Y);

        // Hit both the X and Y coordinate
        // var validStrategies =
        //     xStrategies.Where(x => yStrategies.Any(y => x.APresses == y.APresses && x.BPresses == y.BPresses));
        return xStrategies.DefaultIfEmpty().MinBy(Cost);
    }

    internal static IEnumerable<Strategy> CandidateStrategies(long aIncrement, long bIncrement, long target,
        Func<Strategy, bool> extraCondition)
    {
        // Step 1: Find the greatest common divisor (gcd) of aIncrement and bIncrement using the Euclidean algorithm
        var gcd = GCD(aIncrement, bIncrement);

        // Step 2: If the gcd does not divide the target, there is no solution
        if (target % gcd != 0) yield break; // No solution

        Console.WriteLine("gcd hit");

        // Step 3: Use the extended Euclidean algorithm to find the particular solution
        var (a0, b0) = ExtendedGCD(aIncrement, bIncrement);

        // Step 4: Scale the particular solution to match the target
        a0 *= target / gcd;
        b0 *= target / gcd;

        // Step 5: Now find the general solution
        var stepA = bIncrement / gcd;
        var stepB = aIncrement / gcd;

        // Step 6: Adjust k to find all positive solutions
        // We need k such that both a0 + k * stepA > 0 and b0 - k * stepB > 0
        var kMin = (1 - a0 + stepA - 1) / stepA; // ceil(1 - x0 / stepA)
        var kMax = (b0 - 1) / stepB; // floor(y0 / stepB)

        Console.WriteLine("kMin: " + kMin);
        Console.WriteLine("kMax: " + kMax);

        // Generate all valid solutions where both a0 + k * stepA > 0 and b0 - k * stepB > 0
        for (var k = kMin; k <= kMax && a0 + k * stepA > 0 && b0 - k * stepB > 0; k++)
        {
            if (k % 100_000 == 0) Console.WriteLine(k);
            var x = a0 + k * stepA;
            var y = b0 - k * stepB;

            var strategy = new Strategy(x, y);
            if (extraCondition(strategy))
            {
                Console.WriteLine(strategy);
                yield return strategy;
            }
        }
    }


    // Helper function to compute GCD
    static readonly Dictionary<(long, long), long> GcdCache = new();

    static long GCD(long a, long b)
    {
        var initial = (a, b);
        if (GcdCache.TryGetValue(initial, out var cached)) return cached;
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }

        return GcdCache[initial] = a;
    }

    static readonly Dictionary<(long, long), (long, long)> ExtendedCache = new();

    // Extended Euclidean algorithm to find x0, y0 such that a * x0 + b * y0 = gcd(a, b)
    static (long x, long y) ExtendedGCD(long a, long b)
    {
        var initial = (a, b);
        if (ExtendedCache.TryGetValue(initial, out var cached)) return cached;

        long x0 = 1, y0 = 0, x1 = 0, y1 = 1;
        while (b != 0)
        {
            var q = a / b;
            var r = a % b;

            var tempX = x0 - q * x1;
            var tempY = y0 - q * y1;

            a = b;
            b = r;
            x0 = x1;
            y0 = y1;
            x1 = tempX;
            y1 = tempY;
        }

        return ExtendedCache[initial] = (x0, y0);
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


record Machine(Button A, Button B, Prize Prize);

record Button(long XIncrement, long YIncrement);

record Prize(long X, long Y);

record struct Strategy(long APresses, long BPresses);