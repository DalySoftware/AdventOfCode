// See https://aka.ms/new-console-template for more information

using Wires = System.Collections.Generic.Dictionary<string, bool>;

// var input = """
//             x00: 1
//             x01: 0
//             x02: 1
//             x03: 1
//             x04: 0
//             y00: 1
//             y01: 1
//             y02: 1
//             y03: 1
//             y04: 1
//
//             ntg XOR fgs -> mjb
//             y02 OR x01 -> tnw
//             kwq OR kpj -> z05
//             x00 OR x03 -> fst
//             tgd XOR rvg -> z01
//             vdt OR tnw -> bfw
//             bfw AND frj -> z10
//             ffh OR nrd -> bqk
//             y00 AND y03 -> djm
//             y03 OR y00 -> psh
//             bqk OR frj -> z08
//             tnw OR fst -> frj
//             gnj AND tgd -> z11
//             bfw XOR mjb -> z00
//             x03 OR x00 -> vdt
//             gnj AND wpb -> z02
//             x04 AND y00 -> kjc
//             djm OR pbm -> qhw
//             nrd AND vdt -> hwm
//             kjc AND fst -> rvg
//             y04 OR y02 -> fgs
//             y01 AND x02 -> pbm
//             ntg OR kjc -> kwq
//             psh XOR fgs -> tgd
//             qhw XOR tgd -> z09
//             pbm OR djm -> kpj
//             x03 XOR y03 -> ffh
//             x00 XOR y04 -> ntg
//             bfw OR bqk -> z06
//             nrd XOR fgs -> wpb
//             frj XOR qhw -> z04
//             bqk OR frj -> z07
//             y03 OR x01 -> nrd
//             hwm AND bqk -> z03
//             tgd XOR rvg -> z12
//             tnw OR pbm -> gnj
//             """;


var input = """
            x00: 0
            x01: 1
            x02: 0
            x03: 1
            x04: 0
            x05: 1
            y00: 0
            y01: 0
            y02: 1
            y03: 1
            y04: 0
            y05: 1

            x00 AND y00 -> z05
            x01 AND y01 -> z02
            x02 AND y02 -> z01
            x03 AND y03 -> z03
            x04 AND y04 -> z04
            x05 AND y05 -> z00
            """;

// var input = File.ReadAllText("input.txt");

var device = Parse(input);

// Console.WriteLine("Result:");
// Console.WriteLine(device.Calculate());

var solver = new Solver(device);

Console.WriteLine("Result:");
Console.WriteLine(solver.Solve());

return;

Device Parse(string input)
{
    var lines = input.Split(["\n", "\r\n"], StringSplitOptions.TrimEntries);

    var wires = lines
        .Where(l => l.Contains(':'))
        .Select(line => line.Split(":", StringSplitOptions.TrimEntries))
        .ToDictionary(parts => parts[0], parts => parts[1] == "1");

    var gates = lines
        .Where(l => l.Contains("->"))
        .Select(l =>
        {
            var parts = l.Split(' ');
            return new Gate(parts[0], parts[2], parts[1].ParseOperation(), parts[4]);
        })
        .ToArray();

    return new Device(wires, gates);
}

class Solver(Device initialDevice)
{
    IEnumerable<(Device Device, ((Gate, Gate), (Gate, Gate)) PairOfPairs)> PossibleDevices()
    {
        var pairsOfPairs = initialDevice.Gates.GetPairs();

        foreach (var pairOfPairs in pairsOfPairs)
        {
            Gate[] swappedgates =
                [pairOfPairs.PairA.Item1, pairOfPairs.PairA.Item2, pairOfPairs.PairB.Item1, pairOfPairs.PairB.Item2];
            var unswappedGates = initialDevice.Gates.Where(g => swappedgates.Contains(g));
            var newGates = unswappedGates
                .Append(pairOfPairs.PairA.Item1 with { OutAddress = pairOfPairs.PairA.Item2.OutAddress })
                .Append(pairOfPairs.PairA.Item2 with { OutAddress = pairOfPairs.PairA.Item1.OutAddress })
                .Append(pairOfPairs.PairB.Item2 with { OutAddress = pairOfPairs.PairB.Item1.OutAddress })
                .Append(pairOfPairs.PairB.Item2 with { OutAddress = pairOfPairs.PairB.Item1.OutAddress });
            yield return (new Device(initialDevice.InitialWires, newGates.ToArray()), pairOfPairs);
        }
    }

    internal string Solve()
    {
        var solution = PossibleDevices().First(d => d.Device.HasValidSum());
        Gate[] swapped =
        [
            solution.PairOfPairs.Item1.Item1,
            solution.PairOfPairs.Item1.Item2,
            solution.PairOfPairs.Item2.Item1,
            solution.PairOfPairs.Item2.Item2,
        ];

        return string.Join(",", swapped.Select(g => g.OutAddress).Order());
    }
}

record Device(Wires InitialWires, Gate[] Gates)
{
    readonly Wires _wires = InitialWires.ToDictionary(); // take a copy

    internal long Calculate()
    {
        var toCalculate = new Queue<Gate>(Gates);

        while (toCalculate.TryDequeue(out var gate))
        {
            if (!TryGetOutput(gate, out var output))
            {
                toCalculate.Enqueue(gate);
                continue;
            }

            _wires[gate.OutAddress] = (bool)output!;
        }

        return _wires
            .Where(w => w.Key.StartsWith('z'))
            .OrderBy(w => w.Key)
            .Select(w => w.Value)
            .ToLong();
    }

    internal bool HasValidSum()
    {
        var groups = _wires.ToLookup(kv => kv.Key[0], kv => kv.Value);
        return groups['x'].ToLong() + groups['y'].ToLong() == groups['z'].ToLong();
    }

    bool TryGetOutput(Gate gate, out bool? output)
    {
        if (!_wires.TryGetValue(gate.AddressA, out var operandA) ||
            !_wires.TryGetValue(gate.AddressB, out var operandB))
        {
            output = null;
            return false;
        }

        output = Output(gate.Operation, operandA, operandB);
        return true;
    }

    static bool Output(Operations operation, bool operandA, bool operandB) => operation switch
    {
        Operations.And => operandA && operandB,
        Operations.Or => operandA || operandB,
        Operations.Xor => operandA ^ operandB,
        _ => throw new InvalidOperationException(),
    };
}

static class Extensions
{
    internal static long ToLong(this IEnumerable<bool> bools) =>
        bools
            .Select((b, i) => b ? 1L << i : 0L)
            .Aggregate(0L, (acc, val) => acc | val);


    internal static Operations ParseOperation(this string operationString) => operationString switch
    {
        "AND" => Operations.And,
        "OR" => Operations.Or,
        "XOR" => Operations.Xor,
        _ => throw new ArgumentOutOfRangeException(nameof(operationString), operationString, null),
    };

    public static List<((T, T) PairA, (T, T) PairB)> GetPairs<T>(this IReadOnlyCollection<T> items)
    {
        return items
            .SelectMany((_, index) =>
                    items.Skip(index + 1),
                (item1, item2) => (item1, item2))
            .SelectMany(pair1 =>
                items.Except([pair1.Item1, pair1.Item2])
                    .SelectMany((item, index) =>
                            items.Skip(index + 1)
                                .Except([pair1.Item1, pair1.Item2]),
                        (item3, item4) => (pair1, (item3, item4))))
            .ToList();
    }
}

record Gate(string AddressA, string AddressB, Operations Operation, string OutAddress);

enum Operations
{
    And,
    Or,
    Xor,
}