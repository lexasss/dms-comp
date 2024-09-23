using DmsComparison;
using DmsComparison.Algorithms;

// Options
const bool USE_RECTIFICATION = true;

// Select data folder
var ofd = new OpenFolder.FolderPicker
{
    OkButtonLabel = "Load",
    Title = "Select a folder with DMS data"
};
if (ofd.ShowDialog() == false)
{
    return;
}

var folder = ofd.ResultPath;
var files = Directory.EnumerateFiles(folder, "*.json");

Console.WriteLine($"DMS source folder: {folder}");
Console.WriteLine($"DMS files: {files.Count()}");
Console.WriteLine();

// Load DMS data
var dmsList = new Dictionary<string, HashSet<Dms>>();
foreach (var file in files)
{
    var dms = Dms.Load(file);
    string? mixType = dms?.Info?.Split(",")[0];
    if (dms == null || mixType == null)
        continue;

    if (!dmsList.TryGetValue(mixType, out HashSet<Dms>? set))
    {
        set = ([]);
        dmsList.Add(mixType, set);
    }

    set.Add(dms);
}

// Load algorithms
var algorithms = new List<Algorithm>();
var algorithmTypes = Algorithm.GetDescendantTypes();
foreach (var algorithmType in algorithmTypes)
{
    if (Activator.CreateInstance(algorithmType) is Algorithm algorithm)
    {
        if (algorithm.Name == "DTW")
            continue;
        algorithms.Add(algorithm);
    }
}

// Compute distances
var distances = new Dictionary<Algorithm, Dictionary<string, double>>();

Console.WriteLine("Calculating pairwise distances:");
foreach (var algorithm in algorithms)
{
    Console.Write($"{algorithm.Name} . . . ");

    var results = new Dictionary<string, double>();
    distances.Add(algorithm, results);

    var mixTypes = dmsList.Keys;
    for (int m = 0; m < mixTypes.Count; m++)
    {
        for (int n = m; n < mixTypes.Count; n++)
        {
            var mixType1 = mixTypes.ElementAt(n);
            var mixType2 = mixTypes.ElementAt(m);

            double distanceSum = 0;
            int distanceCount = 0;

            var dmsSet1 = dmsList[mixType1].ToArray();
            var dmsSet2 = dmsList[mixType2].ToArray();
            for (int i = 0; i < dmsSet1.Length - 1; i++)
            {
                for (int j = i + 1; j < dmsSet2.Length; j++)
                {
                    distanceCount += 1;
                    distanceSum += algorithm.ComputeDistance(dmsSet1[i].Data, dmsSet2[j].Data, USE_RECTIFICATION);
                }
            }

            var distance = distanceSum / (distanceCount > 0 ? distanceCount : 1);
            results.Add($"{mixType1[3..]}/{mixType2[3..]}", distance);
        }
    }

    Console.WriteLine("done.");
}

// Print results
const int colSize_first = 14;
const int colSize_rest = 8;

Console.WriteLine();
Console.Write("_");
Console.Write(new string(' ', colSize_first - 1));

foreach (var (_, comparisons) in distances)
{
    foreach (var (pair, _) in comparisons)
    {
        Console.Write($"{pair,-colSize_rest}");
    }
    break;
}
Console.WriteLine();

foreach (var (algorithm, comparisons) in distances)
{
    Console.Write($"{algorithm.Name,-colSize_first}");
    foreach (var (_, distance) in comparisons)
    {
        Console.Write($"{distance,-colSize_rest:F3}");
    }
    Console.WriteLine();
}
