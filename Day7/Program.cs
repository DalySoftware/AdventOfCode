// See https://aka.ms/new-console-template for more information

var testString = """
                 190: 10 19
                 3267: 81 40 27
                 83: 17 5
                 156: 15 6
                 7290: 6 8 6 15
                 161011: 16 10 13
                 192: 17 8 14
                 21037: 9 7 18 13
                 292: 11 6 16 20
                 """;

// var lines = testString.Split(Environment.NewLine);
var lines = File.ReadAllLines("input.txt");

var equations = lines.Select(line =>
{
    var parts = line.Split(":", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    return (
        Total: long.Parse(parts[0]),
        Numbers: parts[1].Split(" ", StringSplitOptions.TrimEntries).Select(long.Parse).ToList()
    );
});

var result = equations
    .Where(e => CanEquate(e.Total, e.Numbers))
    .Sum(e => e.Total);

Console.WriteLine("Result: ");
Console.WriteLine(result);

return;

bool CanEquate(long total, List<long> numbers) => PossibleValues(numbers).Contains(total);

IEnumerable<long> PossibleValues(List<long> numbers)
{
    if (numbers.Count == 1) return [numbers[0]];

    var lastNumber = numbers[^1];
    var otherNumbers = numbers[..^1];
    var otherNumbersValues = PossibleValues(otherNumbers).ToList();
    return
    [
        ..otherNumbersValues.Select(n => n + lastNumber),
        ..otherNumbersValues.Select(n => n * lastNumber),
    ];
}