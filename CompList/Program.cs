using DmsComparison;
using DmsComparison.Algorithms;
using System.Security.Cryptography;

/*
var ofd = new Microsoft.Win32.OpenFolderDialog();
if (ofd.ShowDialog() == false)
{
    return;
}

var folder = ofd.FolderName;*/

var folder = @"C:\Users\olequ\Documents\Data\Smellodi\Naa\Repeated\without-filter";
var files = Directory.EnumerateFiles(folder, "*.json");

// Load DMS data
var dmsList = new Dictionary<string, HashSet<Dms>>();
foreach (var file in files)
{
    var dms = Dms.Load(file);
    string? mixType = dms?.Info?.Split(",")[0];
    if (dms == null || mixType == null)
        continue;

    if (!dmsList.ContainsKey(mixType))
    {
        dmsList.Add(mixType, []);
    }

    var set = dmsList[mixType];
    set.Add(dms);
}

// Load algorithms
var algorithms = new List<Algorithm>();
var algorithmTypes = Algorithm.GetAllAlgorithmTypes();
foreach (var algorithmType in algorithmTypes)
{
    var algorithm = Activator.CreateInstance(algorithmType) as Algorithm;
    if (algorithm != null)
    {
        algorithms.Add(algorithm);
    }
}

// Compute distances
var distances = new Dictionary<Algorithm, Dictionary<string, double>>();

Console.WriteLine("Calculating:");
foreach (var algorithm in algorithms)
{
    Console.Write($"Algorithm: {algorithm.Name} . . . ");

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
                    distanceSum += algorithm.ComputeDistance(dmsSet1[i].Data, dmsSet2[j].Data, false);
                }
            }

            var distance = distanceSum / (distanceCount > 0 ? distanceCount : 1);
            results.Add($"{mixType1[3..]}-{mixType2[3..]}", distance);
        }
    }

    Console.WriteLine("done.");
}

// Print results

Console.Write("              ");
foreach (var (_, comparisons) in distances)
{
    foreach (var (pair, _) in comparisons)
    {
        Console.Write($"{pair,16}");
    }
    break;
}
Console.WriteLine();

foreach (var (algorithm, comparisons) in distances)
{
    Console.Write($"{algorithm.Name,14}");
    foreach (var (_, distance) in comparisons)
    {
        Console.Write($"{distance,16:F3}");
    }
    Console.WriteLine();
}
