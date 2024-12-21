﻿// See https://aka.ms/new-console-template for more information

using System.Text;

var numeric = new NumericKeypad();
var directional = new DirectionalKeypad();

string[] codes =
[
    // "029A", "980A", "179A", "456A", "379A",
    "638A", "965A", "780A", "803A", "246A",
];

var solver = new Solver(directional, numeric);

Console.WriteLine("Result:");
Console.WriteLine(solver.Complexity(codes));

return;

class Solver(DirectionalKeypad directional, NumericKeypad numeric)
{
    internal string NestedSequence(string code, int n)
    {
        var sequences = numeric.Sequences(code);

        for (var i = 0; i < n; i++) sequences = sequences.SelectMany(directional.Sequences);

        return sequences.MinBy(s => s.Length)!;
    }


    internal int Complexity(IEnumerable<string> codes) => codes.Sum(Complexity);

    int Complexity(string code)
    {
        var length = NestedSequence(code, 3).Length;
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

    Dictionary<string, string[]> _targetSequenceCache = new();

    // internal IEnumerable<string> Sequences(string target) => _targetSequenceCache.TryGetValue(target, out var cached)
    //     ? cached
    //     : _targetSequenceCache[target] = UncachedSequences(target).ToArray();

    internal IEnumerable<string> Sequences(string target) => UncachedSequences(target);

    List<string> UncachedSequences(string target)
    {
        Console.WriteLine("UncachedSequences");

        var stack = new Stack<(string Target, StringBuilder Prior, Position CurrentPosition)>();
        var results = new List<string>();

        stack.Push((target, new StringBuilder(), StartingPosition));

        while (stack.TryPop(out var current))
        {
            var (currentTarget, prior, currentPosition) = current;

            if (currentTarget == string.Empty)
            {
                results.Add(prior.ToString()); // Final conversion to string
                continue;
            }

            var currentChar = currentTarget[0];
            var nextCharSequences = CachedSequencesTo(currentPosition, Keys[currentChar]);

            foreach (var sequence in nextCharSequences)
            {
                // Clone the StringBuilder only when pushing to stack
                var sb = new StringBuilder(prior.Length + sequence.Length + 1);
                sb.Append(prior).Append(sequence).Append(Symbols.Push);
                stack.Push((currentTarget[1..], sb, Keys[currentChar]));
            }
        }

        return results;
    }


    readonly Dictionary<(Position, Position), string[]> _positionSequenceCache = new();

    string[] CachedSequencesTo(Position fromPosition, Position toPosition) =>
        _positionSequenceCache.TryGetValue((fromPosition, toPosition), out var cached)
            ? cached
            : _positionSequenceCache[(fromPosition, toPosition)] = SequencesTo(fromPosition, toPosition).ToArray();

    IEnumerable<string> SequencesTo(Position fromPosition, Position toPosition)
    {
        Console.WriteLine("SequencesTo uncached");
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