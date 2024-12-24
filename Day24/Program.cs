// See https://aka.ms/new-console-template for more information

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

var input = File.ReadAllText("input.txt");

var device = Parse(input);

Console.WriteLine("Result:");
Console.WriteLine(device.Calculate());

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

class Device(Dictionary<string, bool> initialWires, Gate[] gates)
{
    readonly Dictionary<string, bool> _wires = initialWires.ToDictionary(); // take a copy

    internal long Calculate()
    {
        var toCalculate = new Queue<Gate>(gates);

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

    bool Output(Operations operation, bool operandA, bool operandB) => operation switch
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
}

record Gate(string AddressA, string AddressB, Operations Operation, string OutAddress);

enum Operations
{
    And,
    Or,
    Xor,
}