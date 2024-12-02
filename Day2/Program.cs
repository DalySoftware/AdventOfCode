// See https://aka.ms/new-console-template for more information

// List<List<int>> reports = [
//     // [7, 6, 4, 2, 1],
//     // [1, 2, 7, 8, 9],
//     // [9, 7, 6, 2, 1],
//     // [1, 3, 2, 4, 5],
//     // [8, 6, 4, 4, 1],
//     // [1, 5, 6, 7] // broken
// ];

var reports = LoadReports();

Console.WriteLine("Result:");
Console.WriteLine(reports.Count(ReportChecker.IsSafe));
return;

IEnumerable<IEnumerable<int>> LoadReports()
{
    var lines = File.ReadAllLines("input.txt");
    return lines
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .Select(line => line.Split(" ").Select(int.Parse));
}

internal static class ReportChecker
{
    private static DirectionAccumulate InitialState => new(Status.Unknown, null);  
    private static DirectionAccumulate CheckReport(this IEnumerable<int> report)
    => report.Aggregate(InitialState, (acc, next) 
        =>
        {
            if (acc.Status == Status.Unsafe)
            {
                return acc;
            }
            
            if (acc is { Status: Status.Unknown, Last: null })
            {
                return new (Status.Unknown, next);
            }

            if (acc.Status == Status.Unknown)
            {
                return (next - acc.Last) switch
                {
                    >= 1 and <= 3 => new DirectionAccumulate(Status.SafeAscending, next),
                    <= -1 and >= -3 => new DirectionAccumulate(Status.SafeDescending, next),
                    _ => new DirectionAccumulate(Status.Unsafe, null),
                };
            }
            
            return acc.Status switch
            {
                // Check difference is between 1 and 3 in the expected direction
                Status.SafeAscending when 1 <= next - acc.Last && next - acc.Last <= 3 => new(Status.SafeAscending,
                    next),
                Status.SafeDescending when 1 <= acc.Last - next && acc.Last - next <= 3 => new(Status.SafeDescending,
                    next),
                _ => new(Status.Unsafe, null)
            };
        });
    
    internal static bool IsSafe(this IEnumerable<int> report)
    {
        if (report.CheckReport().IsSafe)
        {
            return true;
        }

        return report
            .PossibleSublistsExcludingOneItem()
            .Any(subList => subList.CheckReport().IsSafe);
    }

    private static IEnumerable<IEnumerable<T>> PossibleSublistsExcludingOneItem<T>(this IEnumerable<T> original)
        => original.Select((_, i) => original.Where((_, index) => index != i));
}

internal enum Status
{
    Unknown,
    Unsafe,
    SafeAscending,
    SafeDescending,
}

internal record DirectionAccumulate(Status Status, int? Last)
{
    internal bool IsSafe => Status is Status.SafeAscending or Status.SafeDescending;
};