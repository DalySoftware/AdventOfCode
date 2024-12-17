// See https://aka.ms/new-console-template for more information

// var input = """
//             Register A: 729
//             Register B: 0
//             Register C: 0
//
//             Program: 0,1,5,4,3,0
//             """;

var input = """
            Register A: 22817223
            Register B: 0
            Register C: 0

            Program: 2,4,1,2,7,5,4,5,0,3,1,7,5,5,3,0
            """;

var computer = Parse(input);

computer.Execute();

return;

Computer Parse(string input)
{
    var lines = input.Split(["\r\n", "\n"], StringSplitOptions.TrimEntries)
        .ToArray();

    var a = int.Parse(lines[0].Replace("Register A: ", ""));
    var b = int.Parse(lines[1].Replace("Register B: ", ""));
    var c = int.Parse(lines[2].Replace("Register C: ", ""));

    var instructionString = lines[4].Replace("Program: ", "");
    var instructions = instructionString.Split(",").Select(int.Parse).ToArray();

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

record Computer(int[] Inputs, int[] Registers)
{
    int _registerA = Registers[0];
    int _registerB = Registers[1];
    int _registerC = Registers[2];

    int _instructionPointer = 0;

    readonly List<int> _outputs = [];

    internal void Execute()
    {
        while (TryExecuteNext())
        {
        }

        Console.WriteLine("Finished executing");

        if (_outputs.Count > 0) Console.WriteLine(string.Join(",", _outputs));
    }

    bool TryExecuteNext()
    {
        if (_instructionPointer > Inputs.Length - 1) return false;

        var instruction = (Instructions)Inputs[_instructionPointer];
        var operand = Inputs[_instructionPointer + 1];
        var result = Execute(instruction, operand);

        if (result is not Jumped) _instructionPointer += 2;
        if (result is OutputResult output) _outputs.Add(output.Output);

        return true;
    }

    int ComboOperand(int operand) => operand switch
    {
        >= 0 and <= 3 => operand,
        4 => _registerA,
        5 => _registerB,
        6 => _registerC,
        _ => throw new ArgumentOutOfRangeException(nameof(operand)),
    };

    ExecutionResult Execute(Instructions instruction, int literalOperand)
    {
        switch (instruction)
        {
            case Instructions.Adv:
                var advDenominator = (int)Math.Pow(2, ComboOperand(literalOperand));
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
                var bdvDenominator = (int)Math.Pow(2, ComboOperand(literalOperand));
                _registerB = _registerA / bdvDenominator;
                return ExecutionResult.Void;
            case Instructions.Cdv:
                var cdvDenominator = (int)Math.Pow(2, ComboOperand(literalOperand));
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

    record OutputResult(int Output) : ExecutionResult;

    record Jumped : ExecutionResult;
}