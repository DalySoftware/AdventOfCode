// See https://aka.ms/new-console-template for more information

// var input = """
//             Register A: 2024
//             Register B: 0
//             Register C: 0
//
//             Program: 0,3,5,4,3,0
//             """;

var input = """
            Register A: 20291778
            Register B: 0
            Register C: 0

            Program: 2,4,1,2,7,5,4,5,0,3,1,7,5,5,3,0
            """;

var initial = Parse(input);
initial.Execute();

var computerComputer = new ComputerComputer(initial);

var result = computerComputer.LowestCopyValue();

Console.WriteLine("Result: ");
Console.WriteLine(result);

return;

Computer Parse(string input)
{
    var lines = input.Split(["\r\n", "\n"], StringSplitOptions.TrimEntries)
        .ToArray();

    var a = long.Parse(lines[0].Replace("Register A: ", ""));
    var b = long.Parse(lines[1].Replace("Register B: ", ""));
    var c = long.Parse(lines[2].Replace("Register C: ", ""));

    var instructionString = lines[4].Replace("Program: ", "");
    var instructions = instructionString.Split(",").Select(long.Parse).ToArray();

    return new Computer(instructions, [a, b, c]);
}

enum Instructions
{
    Adv = 0,
    Bxl = 1,
    Bst = 2,
    Jnz = 3,
    Bxc = 4,
    Output = 5,
    Bdv = 6,
    Cdv = 7,
}

record ComputerComputer(Computer Initial)
{
    internal long LowestCopyValue()
    {
        Initial.Execute();

        var target = Initial.Inputs;
        var reversedTarget = target.Reverse().ToArray();

        var registersBC = Initial.Registers.Skip(1).ToArray();

        var i = 0;
        List<long> digits = [];

        while (i < target.Length)
        {
            var range = Enumerable.Range(0, 8);

            var digit = range.First(r =>
            {
                var guess = ReadDigits(digits) * 8 + r;
                var computer = new Computer(Initial.Inputs, registersBC.Prepend(guess).ToArray());
                computer.Execute();
                var output = computer.Outputs.ToArray().Reverse().ToArray()[i];
                Console.WriteLine(Format(computer.Outputs.ToArray(), target));
                return output == reversedTarget[i];
            });

            digits.Add(digit);
            Console.WriteLine(ReadDigits(digits));
            i++;
        }

        return ReadDigits(digits);
    }

    long ReadDigits(IEnumerable<long> digits) => digits.Aggregate(0L, (cur, val) => cur * 8 + val);

    internal static string Format(long[] output, long[] target) => string.Join(",", FormatParts(output, target));

    static IEnumerable<string> FormatParts(long[] output, long[] target)
    {
        const string red = "\u001b[31m";
        const string green = "\u001b[32m";
        const string reset = "\u001b[0m";

        for (var i = output.Length; i > 0; i--)
        {
            var color = output[^i] == target[^i] ? green : red;
            yield return color + output[^i] + reset;
        }
    }
}

record Computer(long[] Inputs, long[] Registers)
{
    long _registerA = Registers[0];
    long _registerB = Registers[1];
    long _registerC = Registers[2];

    long _instructionPointer = 0;

    internal List<long> Outputs { get; } = [];

    internal void Execute()
    {
        while (TryExecuteNext())
        {
        }
        // if (Outputs.Count > 0) Console.WriteLine(string.Join(",", Outputs));
    }

    bool TryExecuteNext()
    {
        if (_instructionPointer > Inputs.Length - 1) return false;

        var instruction = (Instructions)Inputs[_instructionPointer];
        var operand = Inputs[_instructionPointer + 1];
        var result = Execute(instruction, operand);

        if (result is not Jumped) _instructionPointer += 2;
        if (result is OutputResult output) Outputs.Add(output.Output);

        return true;
    }

    long ComboOperand(long operand) => operand switch
    {
        >= 0 and <= 3 => operand,
        4 => _registerA,
        5 => _registerB,
        6 => _registerC,
        _ => throw new ArgumentOutOfRangeException(nameof(operand)),
    };

    ExecutionResult Execute(Instructions instruction, long literalOperand)
    {
        switch (instruction)
        {
            case Instructions.Adv:
                var advDenominator = (long)Math.Pow(2, ComboOperand(literalOperand));
                _registerA /= advDenominator;
                return ExecutionResult.Void;
            case Instructions.Bxl:
                _registerB ^= literalOperand;
                return ExecutionResult.Void;
            case Instructions.Bst:
                _registerB = ComboOperand(literalOperand) % 8;
                return ExecutionResult.Void;
            case Instructions.Jnz:
                if (_registerA == 0) return ExecutionResult.Void;
                _instructionPointer = literalOperand;
                return new Jumped();
            case Instructions.Bxc:
                _registerB ^= _registerC;
                return ExecutionResult.Void;
            case Instructions.Output:
                return new OutputResult(ComboOperand(literalOperand) % 8);
            case Instructions.Bdv:
                var bdvDenominator = (long)Math.Pow(2, ComboOperand(literalOperand));
                _registerB = _registerA / bdvDenominator;
                return ExecutionResult.Void;
            case Instructions.Cdv:
                var cdvDenominator = (long)Math.Pow(2, ComboOperand(literalOperand));
                _registerC = _registerA / cdvDenominator;
                return ExecutionResult.Void;
            default:
                throw new ArgumentOutOfRangeException(nameof(instruction), instruction, null);
        }
    }

    abstract record ExecutionResult
    {
        internal static Void Void => new();
    };

    record Void : ExecutionResult;

    record OutputResult(long Output) : ExecutionResult;

    record Jumped : ExecutionResult;
}