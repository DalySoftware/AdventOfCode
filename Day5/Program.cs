// See https://aka.ms/new-console-template for more information

const string testInput = """
                         47|53
                         97|13
                         97|61
                         97|47
                         75|29
                         61|13
                         75|53
                         29|13
                         97|29
                         53|29
                         61|53
                         97|53
                         61|29
                         47|13
                         75|47
                         97|75
                         47|61
                         75|61
                         47|29
                         75|13
                         53|13

                         75,47,61,53,29
                         97,61,53,29,13
                         75,29,13
                         75,97,47,61,53
                         61,13,29
                         97,13,75,29,47
                         """;

var fileInput = File.ReadAllText("input.txt");
var input = Read(fileInput);

var invalidUpdates = input.Updates.Where(u => !u.IsValid(input.Rules));

var fixedUpdates = invalidUpdates.Select(u => FixInvalid(u, input.Rules));

var result = fixedUpdates.Sum(u => MiddleOf(u.Pages));
Console.WriteLine("Total:");
Console.WriteLine(result);

return;

T MiddleOf<T>(T[] array) => array[array.Length / 2];

Input Read(string inputText)
{
    var sections = inputText.Split(["\n\n", "\r\n\r\n"], StringSplitOptions.None);
    var rules = sections[0]
        .Split(["\n", "\r\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select(str =>
        {
            var parts = str.Split("|").Select(int.Parse).ToArray();
            return new Rule(parts[0], parts[1]);
        })
        .ToArray();

    var updates = sections[1]
        .Split(["\n", "\r\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select(str => new Update(str.Split(",").Select(int.Parse).ToArray()))
        .ToArray();

    return new Input(rules, updates);
}

Update FixInvalid(Update update, Rule[] rules)
{
    var failedRule = rules.FirstOrDefault(r => !r.IsSatisfied(update));
    if (failedRule == null)
        // valid update
        return update;

    var newOrder = update.Pages.Where(p => p != failedRule.Right).Append(failedRule.Right);
    var newUpdate = new Update(newOrder.ToArray());

    return FixInvalid(newUpdate, rules);
}


record Input(Rule[] Rules, Update[] Updates);

record Update(int[] Pages)
{
    internal int? PositionOf(int page)
    {
        var index = Array.IndexOf(Pages, page);
        return index == -1 ? null : index;
    }

    internal bool IsValid(params Rule[] rules) => rules.All(r => r.IsSatisfied(this));
};


record Rule(int Left, int Right)
{
    internal bool IsSatisfied(Update update)
    {
        var leftPosition = update.PositionOf(Left);
        var rightPosition = update.PositionOf(Right);

        return (leftPosition ?? int.MinValue) < (rightPosition ?? int.MaxValue);
    }
};