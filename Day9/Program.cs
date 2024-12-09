// See https://aka.ms/new-console-template for more information

// var input = "2333133121414131402";

var input = File.ReadAllLines("input.txt")[0];

var parsed = Parse(input).ToList();

var compacted = Compact(parsed);

Console.WriteLine("Checksum:");
Console.WriteLine(compacted.Checksum());

return;

IEnumerable<Block> Parse(string inputString) =>
    inputString
        .WithIndex()
        .SelectMany<(char Ch, int Index), Block>(x =>
            x.Index % 2 == 0
                ? Enumerable.Repeat(new FileBlock(x.Index.ToId()), x.Ch.AsInt())
                : Enumerable.Repeat(new FreeBlock(), x.Ch.AsInt()));

IReadOnlyCollection<Block> Compact(IReadOnlyCollection<Block> blocks)
{
    var blocksList = blocks.ToList();
    var freeSections = blocksList.FindConsecutiveGroups(b => b is FreeBlock).ToList();
    var fileSections =
        new Stack<Group>(blocksList.FindConsecutiveGroups(b => b is FileBlock));

    foreach (var fileSection in fileSections)
    {
        // blocksList.Print();
        var freeSection = freeSections.FirstOrDefault(
            s => s.Length >= fileSection.Length && s.EndIndex < fileSection.StartIndex);
        if (freeSection is null) continue;

        blocksList.SwapSections(freeSection, fileSection);
        // Recalculate free if we swapped
        freeSections = blocksList.FindConsecutiveGroups(b => b is FreeBlock).ToList();
    }

    return blocksList;
}

abstract record Block;

record FreeBlock : Block;

record FileBlock(long Id) : Block;


static class Extensions
{
    internal static int AsInt(this char ch) => int.Parse(ch.ToString());
    internal static long ToId(this int index) => index / 2;

    internal static IEnumerable<(T Value, int Index)> WithIndex<T>(this IEnumerable<T> enumerable)
        => enumerable.Select((value, index) => (value, index));

    static void SwapBlocks(
        this IList<Block> blocks,
        int indexA,
        int indexB) =>
        (blocks[indexA], blocks[indexB]) = (blocks[indexB], blocks[indexA]);

    internal static void SwapSections(this IList<Block> blocks, Group freeSection,
        Group fileSection)
    {
        if (freeSection.Length < fileSection.Length)
            throw new InvalidOperationException();

        for (var i = 0; i < fileSection.Length; i++)
        {
            var indexA = fileSection.StartIndex + i;
            var indexB = freeSection.StartIndex + i;

            blocks.SwapBlocks(indexA, indexB);
        }
    }

    internal static long Checksum(this IReadOnlyCollection<Block> blocks)
        => blocks
            .WithIndex()
            .Sum(x => x.Value is FileBlock f ? f.Id * x.Index : 0);

    internal static void Print(this IEnumerable<Block> blocks)
    {
        var strings = blocks.Select(b => b is FileBlock f ? f.Id.ToString() : ".");
        Console.WriteLine(string.Concat(strings));
    }

    internal static IEnumerable<Group> FindConsecutiveGroups(this IEnumerable<Block> input,
        Func<Block, bool> matchPredicate)
        =>
            input
                .WithIndex()
                .Aggregate(
                    new List<(Group Group, Block Item)>(),
                    (list, item) =>
                    {
                        if (!matchPredicate(item.Value)) return list;

                        var lastGroup = list.LastOrDefault();
                        if (list.Count == 0 || lastGroup.Group.EndIndex != item.Index - 1 ||
                            !Equals(lastGroup.Item, item.Value))
                        {
                            list.Add((new(item.Index, item.Index), item.Value));
                            return list;
                        }

                        // Replace the last item. 
                        list[^1] = (lastGroup.Group with { EndIndex = item.Index }, item.Value);
                        return list;
                    }
                )
                .Select(x => x.Group);
}

record Group(int StartIndex, int EndIndex)
{
    internal int Length => EndIndex - StartIndex + 1;
};