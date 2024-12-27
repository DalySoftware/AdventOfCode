// See https://aka.ms/new-console-template for more information

// var input = """
//             #####
//             .####
//             .####
//             .####
//             .#.#.
//             .#...
//             .....
//
//             #####
//             ##.##
//             .#.##
//             ...##
//             ...#.
//             ...#.
//             .....
//
//             .....
//             #....
//             #....
//             #...#
//             #.#.#
//             #.###
//             #####
//
//             .....
//             .....
//             #.#..
//             ###..
//             ###.#
//             ###.#
//             #####
//
//             .....
//             .....
//             .....
//             #....
//             #.#..
//             #.#.#
//             #####
//             """;

var input = File.ReadAllText("input.txt");

var state = Parse(input);

Console.WriteLine(CountMatchingPairs(state));

return;


int CountMatchingPairs(State state) => state.Locks.Sum(l => state.Keys.Count(l.Accepts));

State Parse(string input)
{
    var lines = input.Split(["\n", "\r\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    var chunks = lines.Chunk(7).Select(c => c.ToCharMatrix()).ToArray();
    var locks = chunks
        .Where(m => m[0][0] == '#')
        .Select(ParseLock)
        .ToArray();

    var keys = chunks
        .Where(m => m[0][0] == '.')
        .Select(ParseKey)
        .ToArray();

    return new State(keys, locks);
}

Key ParseKey(char[][] charMatrix)
{
    var lengths = new int[5];
    for (var i = 0; i < 5; i++) lengths[i] = charMatrix[i].Count(ch => ch == '#');

    return new(lengths);
}

Lock ParseLock(char[][] charMatrix)
{
    var lengths = new int[5];
    for (var i = 0; i < 5; i++) lengths[i] = charMatrix[i].Count(ch => ch == '#');

    return new(lengths);
}

static class Extensions
{
    internal static char[][] ToCharMatrix(this string[] lines) => lines
        .Select(line => line.ToCharArray())
        .ToArray()
        .Transpose();

    static T[][] Transpose<T>(this T[][] squareMatrix)
    {
        var numCols = squareMatrix[0].Length;
        var transposedArray = new T[numCols][];

        var numRows = squareMatrix.Length;
        for (var i = 0; i < numCols; i++)
        {
            transposedArray[i] = new T[numRows];
            for (var j = 0; j < numRows; j++)
                transposedArray[i][j] = squareMatrix[j][i];
        }

        return transposedArray;
    }
}

record State(Key[] Keys, Lock[] Locks);

record Key(int[] Lengths);

record Lock(int[] Lengths)
{
    internal bool Accepts(Key key)
    {
        var result = key
            .Lengths
            .Select((length, index) => (length, index))
            .All(x => Lengths[x.index] + x.length <= 7);
        return result;
    }
};