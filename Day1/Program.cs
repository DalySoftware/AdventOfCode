// See https://aka.ms/new-console-template for more information

// List<int> left = [3, 4, 2, 1, 3, 3];
// List<int> right = [4, 3, 5, 3, 9, 3];

var fileText = File.ReadAllText("input.txt");
var lines = fileText.Split("\n").Where(s => !string.IsNullOrWhiteSpace(s));
List<int> left = [];
List<int> right = [];
foreach (var line in lines)
{
    var values = line.Split("   ").Select(int.Parse).ToList();
    left.Add(values[0]);
    right.Add(values[1]);
}

CalculateSimilarity();
return;

void CalculateDifferences()
{
    var differences = left.Order().Zip(right.Order(), (l, r) => Math.Abs(l - r));

    Console.WriteLine("Result:");
    Console.WriteLine(differences.Sum());
}

void CalculateSimilarity()
{
    var similarityTotal = left.Sum(l => right.Count(r => l == r) * l);

    Console.WriteLine("Result:");
    Console.WriteLine(similarityTotal);
}
