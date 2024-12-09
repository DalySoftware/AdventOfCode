// See https://aka.ms/new-console-template for more information

// var input = "2333133121414131402";

var input = File.ReadAllLines("input.txt")[0];

var parsed = Parse(input).ToList();

var compacted = Compact(parsed);

Console.WriteLine("Checksum:");
Console.WriteLine(compacted.Checksum());

return;

IEnumerable<IBlock> Parse(string inputString) =>
    inputString
        .WithIndex()
        .SelectMany<(char Ch, int Index), IBlock>(x =>
            x.Index % 2 == 0
                ? Enumerable.Repeat(new FileBlock(x.Index.ToId()), x.Ch.AsInt())
                : Enumerable.Repeat(new FreeBlock(), x.Ch.AsInt()));

IReadOnlyCollection<IBlock> Compact(IReadOnlyCollection<IBlock> blocks)
{
    var swaps = 0;
    while (true)
    {
        var lastFileIndex = blocks.WithIndex().Last(x => x.Value is FileBlock).Index;
        var firstFreeIndex = blocks.WithIndex().First(x => x.Value is FreeBlock).Index;

        if (firstFreeIndex > lastFileIndex) return blocks;

        blocks = blocks.SwapBlocks(lastFileIndex, firstFreeIndex);
        swaps++;
        if (swaps % 1000 == 0) Console.WriteLine("Swaps " + swaps);
    }
}

interface IBlock;

record FreeBlock : IBlock;

record FileBlock(long Id) : IBlock;


static class Extensions
{
    internal static int AsInt(this char ch) => int.Parse(ch.ToString());
    internal static long ToId(this int index) => index / 2;

    internal static IEnumerable<(T Value, int Index)> WithIndex<T>(this IEnumerable<T> enumerable)
        => enumerable.Select((value, index) => (value, index));

    internal static IReadOnlyCollection<IBlock> SwapBlocks(
        this IReadOnlyCollection<IBlock> blocks,
        int indexA,
        int indexB)
    {
        var list = blocks.ToList();
        (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
        return list;
    }

    internal static long Checksum(this IReadOnlyCollection<IBlock> blocks)
        => blocks
            .OfType<FileBlock>()
            .WithIndex()
            .Sum(x => x.Value.Id * x.Index);
}