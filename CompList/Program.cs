using DmsComparison;
using DmsComparison.Algorithms;

// Options
const bool VERBOSE = false;
const bool USE_RECTIFICATION = true;
const Normalization.Type NORMALIZATION_TYPE = Normalization.Type.MinMax;

Console.WriteLine("Conditions");
Console.WriteLine($"  Verbose: {VERBOSE}");
Console.WriteLine($"  Rectification: {USE_RECTIFICATION}");
Console.WriteLine($"  Normalization: {NORMALIZATION_TYPE}");
Console.WriteLine();

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
var mixDmsSet = new Dictionary<string, HashSet<Dms>>();
foreach (var file in files)
{
    var dms = Dms.Load(file);
    string? mixType = dms?.Info?.Split(",")[0];
    if (dms == null || mixType == null)
        continue;

    if (!mixDmsSet.TryGetValue(mixType, out HashSet<Dms>? set))
    {
        set = ([]);
        mixDmsSet.Add(mixType, set);
    }

    set.Add(dms);

    if (VERBOSE)
    {
        Console.WriteLine($"Loading {file}");
    }
}

if (VERBOSE)
{
    Console.WriteLine($"Sets:");
    foreach (var (mix, list) in mixDmsSet)
    {
        Console.WriteLine($"  Mix: {mix}");
        foreach (var dms in list)
        {
            Console.WriteLine($"    {dms.Filename}");
        }
    }
}


// Load algorithms
var algorithms = new List<Algorithm>();
var algorithmTypes = Algorithm.GetDescendantTypes();
foreach (var algorithmType in algorithmTypes)
{
    if (Activator.CreateInstance(algorithmType) is Algorithm algorithm)
    {
        if (VERBOSE && algorithm.Name == "DTW")
            continue;
        algorithms.Add(algorithm);
    }
}

// Compute distances
var distances = new Dictionary<Algorithm, Dictionary<string, double>>();

Console.WriteLine("Calculating pairwise distances:");
foreach (var algorithm in algorithms)
{
    if (VERBOSE)
    {
        Console.WriteLine($"{algorithm.Name} . . . ");
        System.Diagnostics.Debug.WriteLine($"{algorithm.Name} . . . ");
    }
    else
    {
        Console.Write($"{algorithm.Name} . . . ");
    }

    var results = new Dictionary<string, double>();
    distances.Add(algorithm, results);

    var mixTypes = mixDmsSet.Keys;
    for (int m = 0; m < mixTypes.Count; m++)
    {
        for (int n = m; n < mixTypes.Count; n++)
        {
            var mixType1 = mixTypes.ElementAt(n);
            var mixType2 = mixTypes.ElementAt(m);

            if (VERBOSE)
            {
                Console.WriteLine($"{mixType1} vs {mixType2}");
            }

            double distanceSum = 0;
            int distanceCount = 0;

            var dmsSet1 = mixDmsSet[mixType1].ToArray();
            var dmsSet2 = mixDmsSet[mixType2].ToArray();
            for (int i = 0; i < dmsSet1.Length; i++)
            {
                var jStart = mixType1 == mixType2 ? i + 1 : 0;
                for (int j = jStart; j < dmsSet2.Length; j++)
                {
                    var dms1 = dmsSet1[i];
                    var dms2 = dmsSet2[j];
                    if (dms1 == dms2)
                        continue;

                    distanceCount += 1;
                    var dist = algorithm.ComputeDistance(
                        dms1.Data, dms2.Data,
                        USE_RECTIFICATION, NORMALIZATION_TYPE);
                    distanceSum += dist;

                    if (VERBOSE)
                    {
                        Console.WriteLine($"{dms1.Filename}/{dms2.Filename} {mixType1[3..]}/{mixType2[3..]}  {dist:F4}");
                        System.Diagnostics.Debug.WriteLine($"{mixType1[3..]}/{mixType2[3..]}  {dist:F4}");
                    }
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
