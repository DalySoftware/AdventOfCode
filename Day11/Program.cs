// See https://aka.ms/new-console-template for more information

// var testInput = "125 17";

var input = "0 89741 316108 7641 756 9 7832357 91";

var stones = Parse(input);
var finalStones = Blink(stones, 25);

Console.WriteLine("Stones:");
Console.WriteLine(finalStones.Count);

return;

List<long> Blink(List<long> stones, int blinks)
{
    var result = stones;
    while (blinks > 0)
    {
        result = result.SelectMany(Transform).ToList();
        blinks--;
    }

    return result;
}

List<long> Parse(string input)
    => input.Split(" ").Select(long.Parse).ToList();

List<long> Transform(long stone)
{
    return stone switch
    {
        0 => [1],
        _ when stone.HasEvenDigits() => stone.Split(),
        _ => [stone * 2024],
    };
}

static class Extensions
{
    internal static List<long> Split(this long stone)
    {
        var str = stone.ToString();

        var midPoint = str.Length / 2;
        List<string> newStrings = [str[..midPoint], str[midPoint..]];

        return newStrings.Select(long.Parse).ToList();
    }

    internal static bool HasEvenDigits(this long stone)
        => stone.ToString().Length % 2 == 0;
}