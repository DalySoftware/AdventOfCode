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
        // Step 1: Find the greatest common divisor (gcd) of aIncrement and bIncrement
        var gcd = GCD(aIncrement, bIncrement);

        // Step 2: Check if the target is divisible by gcd
        if (target % gcd != 0) yield break; // No solution

        Console.WriteLine("gcd hit");

        // Step 3: Scale coefficients and target by gcd
        var scaleFactor = target / gcd;
        aIncrement /= gcd;
        bIncrement /= gcd;

        // Step 4: Use the extended Euclidean algorithm to find a particular solution
        var (x0, y0) = ExtendedGCD(aIncrement, bIncrement);
        x0 *= scaleFactor;
        y0 *= scaleFactor;

        // Step 5: Define step sizes for the general solution
        var xStep = bIncrement; // Step size for x
        var yStep = aIncrement; // Step size for y

        // Step 6: Determine bounds for k such that x and y remain non-negative
        var kMin = (long)Math.Ceiling((1.0 - x0) / xStep); // Ensure x > 0
        var kMax = (long)Math.Floor((y0 - 1.0) / yStep); // Ensure y > 0

        // If bounds are invalid, no solutions exist
        if (kMin > kMax) yield break;

        // Step 7: Generate valid solutions within bounds
        for (var k = kMin; k <= kMax; k++)
        {
            if (k % 10_000_000 == 0) Console.WriteLine(k);
            var x = x0 + k * xStep;
            var y = y0 - k * yStep;

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