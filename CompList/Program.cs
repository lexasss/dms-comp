namespace CompList;

using DmsComparison;
using DmsComparison.Algorithms;
using System;
using System.Linq;

using MixtureDatasets = KeyValuePair<string, HashSet<DmsComparison.Dms>>;

class Options(bool useRectification, Normalization.Type normalizationType)
{
    public bool UseRectification => useRectification;
    public Normalization.Type NormalizationType => normalizationType;
    public override string ToString()
    {
        string rect = UseRectification ? "T" : "F";
        return $"{rect},{NormalizationType}";
    }
}
record MixtureID(string Id);
record ComparisonResult(string Mixture, double Value);
record TestResult(Options Options, Algorithm Algorithm, ComparisonResult[] Comparisons);

public class Program
{
    const bool VERBOSE = false;
    const bool DEBUG = true;

    public static void Main()
    {
        var files = SelectFolder();
        if (files == null)
            return;

        var mixDmsSets = LoadData(files);

        if (VERBOSE)
        {
            Console.WriteLine($"\nSets:");
            foreach (var (mix, list) in mixDmsSets)
            {
                Console.WriteLine($"  Mix: {mix}");
                foreach (var dms in list)
                {
                    Console.WriteLine($"    {dms.Time}");
                }
            }
        }

        var algorithms = LoadAlgorithms();

        Options[] options = [
            new Options(false, Normalization.Type.None),
            new Options(true, Normalization.Type.None),
            new Options(false, Normalization.Type.MinMax),
            new Options(true, Normalization.Type.MinMax)
        ];

        var results = Run(options, algorithms.ToArray(), mixDmsSets);

        var f = results.First();
        PrintHeader(f.Comparisons);
        PrintByOption(results);

        Console.WriteLine();
        PrintHeader(f.Comparisons);
        PrintByAlgorithm(results);
    }

    const int COLSIZE_ALG = 14;
    const int COLSIZE_DST = 8;

    private static string[]? SelectFolder()
    {
        var ofd = new OpenFolder.FolderPicker
        {
            OkButtonLabel = "Load",
            Title = "Select a folder with DMS data"
        };
        if (ofd.ShowDialog() == false)
        {
            return null;
        }

        var folder = ofd.ResultPath;
        var files = Directory.EnumerateFiles(folder, "*.json");

        Console.WriteLine($"DMS source folder: {folder}");
        Console.WriteLine($"DMS files: {files.Count()}");
        Console.WriteLine();

        return files.ToArray();
    }

    /// <param name="files">a list of full pathes to JSON files</param>
    /// <returns>list of key-value pairs where key is the mixture type, 
    /// and value is a set of measurements of this mixture type</returns>
    private static MixtureDatasets[] LoadData(string[] files)
    {
        var result = new Dictionary<string, HashSet<Dms>>();
        foreach (var file in files)
        {
            var dms = Dms.Load(file);
            string? mixType = dms?.Info?.Split(",")[0][3..];
            if (dms == null || mixType == null)
                continue;

            if (!result.TryGetValue(mixType, out HashSet<Dms>? set))
            {
                set = ([]);
                result.Add(mixType, set);
            }

            set.Add(dms);

            if (VERBOSE)
                Console.WriteLine($"Loading {file}");
        }

        return result.ToArray();
    }

    private static Algorithm[] LoadAlgorithms()
    {
        var result = new List<Algorithm>();
        var algorithmTypes = Algorithm.GetDescendantTypes();
        foreach (var algorithmType in algorithmTypes)
        {
            if (Activator.CreateInstance(algorithmType) is Algorithm algorithm)
            {
                if (DEBUG && algorithm.Name == "DTW")
                    continue;
                result.Add(algorithm);
            }
        }

        return result.ToArray();
    }

    private static TestResult Test(
        Options options,
        Algorithm algorithm,
        MixtureDatasets[] allMixDatasets)
    {
        Console.Write($"{algorithm.Name} . . . ");
        if (VERBOSE)
            Console.WriteLine();

        var results = new List<ComparisonResult>();

        var mixTypes = allMixDatasets.Select(kv => kv.Key);
        for (int m = 0; m < mixTypes.Count(); m++)
        {
            for (int n = m; n < mixTypes.Count(); n++)
            {
                var mixType1 = mixTypes.ElementAt(n);
                var mixType2 = mixTypes.ElementAt(m);

                if (VERBOSE)
                    Console.WriteLine($"  {mixType1} vs {mixType2}");

                double distanceSum = 0;
                int distanceCount = 0;

                var dmsSet1 = allMixDatasets.First(kv => kv.Key == mixType1).Value.ToArray();
                var dmsSet2 = allMixDatasets.First(kv => kv.Key == mixType2).Value.ToArray();
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
                            options.UseRectification,
                            options.NormalizationType);
                        distanceSum += dist;

                        if (VERBOSE)
                            Console.WriteLine($"    {dms1.Time} / {dms2.Time}   =>   {dist:F4}");
                    }
                }

                var distance = distanceSum / (distanceCount > 0 ? distanceCount : 1);
                results.Add(new ComparisonResult($"{mixType1}/{mixType2}", distance));
            }
        }

        Console.WriteLine("  done.");
        return new TestResult(options, algorithm, results.ToArray());
    }

    private static TestResult[] Run(
        Options[] optionsSet,
        Algorithm[] algorithms,
        MixtureDatasets[] mixDataset)
    {
        var distances = new List<TestResult>();

        foreach (var options in optionsSet)
        {
            Console.WriteLine();
            Console.WriteLine("Conditions");
            Console.WriteLine($"  Rectification: {options.UseRectification}");
            Console.WriteLine($"  Normalization: {options.NormalizationType}");

            foreach (var algorithm in algorithms)
            {
                var results = Test(options, algorithm, mixDataset);
                distances.Add(results);
            }
        }

        return distances.ToArray();
    }

    private static void PrintHeader(ComparisonResult[] comparisonResults)
    {
        Console.WriteLine();
        Console.Write("_");
        Console.Write(new string(' ', COLSIZE_ALG - 1));

        foreach (var (pair, _) in comparisonResults)
        {
            Console.Write($"{pair,-COLSIZE_DST}");
        }
        Console.WriteLine();
    }

    private static void PrintByOption(TestResult[] results)
    {
        var list = new Dictionary<Algorithm, List<string>>();
        foreach (var testResult in results)
        {
            if (!list.TryGetValue(testResult.Algorithm, out List<string>? items))
            {
                items = [];
                list[testResult.Algorithm] = items;
            }

            string s = $"{testResult.Options,-COLSIZE_ALG}";
            foreach (var (_, distance) in testResult.Comparisons)
            {
                s += $"{distance,-COLSIZE_DST:F4}";
            }

            items.Add(s);
        }

        foreach (var (alg, lines) in list)
        {
            Console.WriteLine(alg.Name);
            foreach (var line in lines)
                Console.WriteLine(line);
        }
    }

    private static void PrintByAlgorithm(TestResult[] results)
    {
        var list = new Dictionary<Options, List<string>>();
        foreach (var testResult in results)
        {
            if (!list.TryGetValue(testResult.Options, out List<string>? items))
            {
                items = [];
                list[testResult.Options] = items;
            }

            string s = $"{testResult.Algorithm.Name,-COLSIZE_ALG}";
            foreach (var (_, distance) in testResult.Comparisons)
            {
                s += $"{distance,-COLSIZE_DST:F4}";
            }

            items.Add(s);
        }

        foreach (var (opt, lines) in list)
        {
            Console.WriteLine(opt);
            foreach (var line in lines)
                Console.WriteLine(line);
        }
    }
}