// See https://aka.ms/new-console-template for more information

using System.Text;

var numeric = new NumericKeypad();
var directional = new DirectionalKeypad();

string[] codes =
[
    // "029A", "980A", "179A", "456A", "379A",
    "638A", "965A", "780A", "803A", "246A",
];

var solver = new Solver(directional, numeric);
solver.NestedSequence(codes[0]);

Console.WriteLine("Result:");
Console.WriteLine(solver.Complexity(codes));

return;

class Solver(DirectionalKeypad directional, NumericKeypad numeric)
{
    internal string NestedSequence(string code) =>
        numeric.Sequences(code)
            .SelectMany(x1 => directional.Sequences(x1).SelectMany(x2 => directional.Sequences(x2)))
            .MinBy(s => s.Length)!;

    internal int Complexity(IEnumerable<string> codes) => codes.Sum(Complexity);

    int Complexity(string code)
    {
        var length = NestedSequence(code).Length;
        var numericPart = NumericPart(code);
        Console.WriteLine(length + " * " + numericPart);

        return length * numericPart;
    }

    static int NumericPart(string code) => int.Parse(code[..^1]);
}


record struct Position(int X, int Y);

abstract class Keypad
{
    protected abstract Dictionary<char, Position> Keys { get; }
    HashSet<Position> ValidPositions => Keys.Values.ToHashSet(); // this is not ideal for performance

    Position StartingPosition => Keys[Symbols.Push];

    internal IEnumerable<string> Sequences(string target) => Sequences(target, "", StartingPosition);

    IEnumerable<string> Sequences(string target, string prior, Position currentPosition)
    {
        if (target == string.Empty)
        {
            yield return prior;
            yield break;
        }

        var currentChar = target[0];
        var nextCharSequences = CachedSequencesTo(currentPosition, Keys[currentChar]);
        foreach (var x in nextCharSequences.SelectMany(s =>
                     Sequences(target[1..], prior + s + Symbols.Push, Keys[currentChar])))
            yield return x;
    }

    readonly Dictionary<(Position, Position), string[]> _cache = new();

    string[] CachedSequencesTo(Position fromPosition, Position toPosition) =>
        _cache.TryGetValue((fromPosition, toPosition), out var cached)
            ? cached
            : _cache[(fromPosition, toPosition)] = SequencesTo(fromPosition, toPosition).ToArray();

    IEnumerable<string> SequencesTo(Position fromPosition, Position toPosition)
    {
        var sequence = new StringBuilder();
        var xDiff = toPosition.X - fromPosition.X;
        var yDiff = toPosition.Y - fromPosition.Y;

        var currentPosition = fromPosition;

        var horizontalComponent = fromPosition with { X = toPosition.X };
        var hasHorizontalBlocker = !ValidPositions.Contains(horizontalComponent);
        var verticalComponent = fromPosition with { Y = toPosition.Y };
        var hasVerticalBlocker = !ValidPositions.Contains(verticalComponent);

        if (hasHorizontalBlocker)
        {
            foreach (var s in CachedSequencesTo(fromPosition, verticalComponent).SelectMany(first =>
                         CachedSequencesTo(verticalComponent, toPosition).Select(second => first + second)))
                yield return s;
            yield break;
        }

        if (hasVerticalBlocker)
        {
            foreach (var s in CachedSequencesTo(fromPosition, horizontalComponent).SelectMany(first =>
                         CachedSequencesTo(horizontalComponent, toPosition).Select(second => first + second)))
                yield return s;
            yield break;
        }

        if (xDiff != 0 && yDiff != 0)
        {
            foreach (var s in CachedSequencesTo(fromPosition, verticalComponent).SelectMany(first =>
                         CachedSequencesTo(verticalComponent, toPosition).Select(second => first + second)))
                yield return s;
            foreach (var s in CachedSequencesTo(fromPosition, horizontalComponent).SelectMany(first =>
                         CachedSequencesTo(horizontalComponent, toPosition).Select(second => first + second)))
                yield return s;

            yield break;
        }

        while (Math.Abs(xDiff) > 0 || Math.Abs(yDiff) > 0)
        {
            var yIncrement = Math.Sign(yDiff);
            if (yIncrement != 0)
            {
                var verticalTry = currentPosition with { Y = currentPosition.Y + yIncrement };
                if (ValidPositions.Contains(verticalTry))
                {
                    currentPosition = verticalTry;
                    yDiff -= yIncrement;
                    sequence.Append(yIncrement.VerticalSymbol());
                    continue;
                }
            }

            var xIncrement = Math.Sign(xDiff);
            if (xIncrement != 0)
            {
                var horizontalTry = currentPosition with { X = currentPosition.X + xIncrement };
                if (ValidPositions.Contains(horizontalTry))
                {
                    currentPosition = horizontalTry;
                    xDiff -= xIncrement;
                    sequence.Append(xIncrement.HorizontalSymbol());
                    continue;
                }
            }

            throw new Exception("Couldn't move vertically or horizontally");
        }

        yield return sequence.ToString();
    }
}


class NumericKeypad : Keypad
{
    protected override Dictionary<char, Position> Keys { get; } = new()
    {
        ['7'] = new(0, 0),
        ['8'] = new(1, 0),
        ['9'] = new(2, 0),
        ['4'] = new(0, 1),
        ['5'] = new(1, 1),
        ['6'] = new(2, 1),
        ['1'] = new(0, 2),
        ['2'] = new(1, 2),
        ['3'] = new(2, 2),
        ['0'] = new(1, 3),
        ['A'] = new(2, 3),
    };
}

class DirectionalKeypad : Keypad
{
    protected override Dictionary<char, Position> Keys { get; } = new()
    {
        ['^'] = new(1, 0),
        ['A'] = new(2, 0),
        ['<'] = new(0, 1),
        ['v'] = new(1, 1),
        ['>'] = new(2, 1),
    };
}

static class Symbols
{
    internal static char Push => 'A';

    internal static string HorizontalSymbol(this int increment) => increment switch
    {
        > 0 => ">",
        < 0 => "<",
        _ => throw new ArgumentException("Invalid", nameof(increment)),
    };

    internal static string VerticalSymbol(this int increment) => increment switch
    {
        > 0 => "v",
        < 0 => "^",
        _ => throw new ArgumentException("Invalid", nameof(increment)),
    };
}