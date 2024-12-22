﻿// See https://aka.ms/new-console-template for more information

using System.Text;
using Microsoft.Extensions.ObjectPool;

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

    static readonly ObjectPool<StringBuilder> _sbPool =
        new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

    IEnumerable<string> UncachedSequences(string target)
    {
        var stack = new Queue<(string Target, StringBuilder Prior, Position CurrentPosition)>();

        // Get the initial StringBuilder from the pool
        var initialSb = _sbPool.Get();
        stack.Enqueue((target, initialSb, StartingPosition));

        try
        {
            while (stack.TryDequeue(out var current))
            {
                var (currentTarget, prior, currentPosition) = current;

                if (currentTarget == "")
                {
                    yield return prior.ToString();
                    _sbPool.Return(prior); // Return to the pool
                    continue;
                }

                var currentChar = currentTarget[0];
                var nextCharSequences = CachedSequencesTo(currentPosition, Keys[currentChar]);

                foreach (var sequence in nextCharSequences)
                {
                    // Get a new StringBuilder from the pool
                    var sb = _sbPool.Get();
                    sb.Append(prior).Append(sequence).Append(Symbols.Push);

                    stack.Enqueue((currentTarget[1..], sb, Keys[currentChar]));
                }

                _sbPool.Return(prior); // Return the builder back to the pool after use
            }
        }
        finally
        {
            _sbPool.Return(initialSb); // Ensure cleanup even on exception
        }
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