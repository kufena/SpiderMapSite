// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var alllines = File.ReadAllLines(args[0]);
Dictionary<string, int> counts = new Dictionary<string, int>();

foreach (var line in alllines)
{
    var splits = line.Split(",");
    if (splits.Length > 3)
    {
        // should be date, determined, thought to be, comment
        var thoughtToBe = splits[2].ToLower().Trim();
        if (counts.ContainsKey(thoughtToBe)) counts[thoughtToBe] += 1;
        else counts.Add(thoughtToBe, 1);
    }
}

Console.WriteLine($"Actual different keys = {counts.Keys.Count}");
var keys = counts.Keys.ToArray();
Array.Sort(keys);

foreach (var key in keys)
{
    Console.WriteLine($"{key} = {counts[key]}");
}