namespace CompList;

using DmsComparison;
using DmsComparison.Algorithms;
using System;
using System.Linq;

// A key-value pair where the key is the mixture type, 
// and the value is a set of measurements of this mixture type
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
class PairOfMixtures(string mix1, string mix2)
{
    public string Id => $"{mix1}/{mix2}";
    public bool IsSame => mix1 == mix2;
    public override string ToString() => Id;
}
record ComparisonResult(PairOfMixtures Mixtures, double Mean, double Std, double[] Distances);
record TestResult(Options Options, Algorithm Algorithm, ComparisonResult[] Comparisons);

enum PrintedInfo
{
    Mean,
    Std
}

public class Program
{
    const bool VERBOSE = false;
    const bool DEBUG = false;

    const int COLSIZE_COND = 14;
    const int COLSIZE_DIST = 8;

    public static void Main()
    {
        var files = SelectFolder();
        if (files == null)
            return;

        var mixDatasets = LoadData(files);

        if (VERBOSE)
        {
            Console.WriteLine($"\nSets:");
            foreach (var (mix, list) in mixDatasets)
            {
                Console.WriteLine($"  Mix: {mix}");
                foreach (var dms in list)
                {
                    Console.WriteLine($"    {dms.Time}");
                }
            }
        }

        var algorithms = LoadAlgorithms();
        var options = PermutateOptions();

        var results = Run(options, algorithms, mixDatasets);

        var headers = results.First().Comparisons.Select(c => c.Mixtures).ToArray();

        PrintHeader(headers, ["Options", "Algorithm"]);
        PrintByOption(results, PrintedInfo.Mean);
        PrintByOption(results, PrintedInfo.Std);

        PrintHeader(headers, ["Algorithms", "Options"]);
        PrintByAlgorithm(results, PrintedInfo.Mean);
        PrintByAlgorithm(results, PrintedInfo.Std);

        PrintSuspiciousPairs(results);
    }

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

        return files.ToArray();
    }

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

    private static Options[] PermutateOptions() => [
        new Options(false, Normalization.Type.None),
        new Options(true, Normalization.Type.None),
        new Options(false, Normalization.Type.Linear),
        new Options(true, Normalization.Type.Linear),
        new Options(false, Normalization.Type.MinMax),
        new Options(true, Normalization.Type.MinMax)
    ];

    private static TestResult Test(
        Options options,
        Algorithm algorithm,
        MixtureDatasets[] allMixDatasets)
    {
        Console.Write($"{algorithm.Name,-15}");
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
                var distances = new List<double>();

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
                        distances.Add(dist);

                        if (VERBOSE)
                            Console.WriteLine($"    {dms1.Time} / {dms2.Time}   =>   {dist:F4}");
                    }
                }

                var mean = distanceSum / (distanceCount > 0 ? distanceCount : 1);
                var std = Math.Sqrt(distances.Sum(dist => (dist - mean) * (dist - mean)) / (distances.Count - 1));

                results.Add(new ComparisonResult(new PairOfMixtures(mixType1, mixType2), mean, std, distances.ToArray()));
            }
        }

        Console.WriteLine("  done");
        return new TestResult(options, algorithm, results.ToArray());
    }

    private static TestResult[] Run(
        Options[] optionsSet,
        Algorithm[] algorithms,
        MixtureDatasets[] mixDataset)
    {
        var results = new List<TestResult>();

        foreach (var options in optionsSet)
        {
            Console.WriteLine();
            Console.WriteLine("Conditions");
            Console.WriteLine($"  Rectification: {options.UseRectification}");
            Console.WriteLine($"  Normalization: {options.NormalizationType}");

            foreach (var algorithm in algorithms)
            {
                var testResult = Test(options, algorithm, mixDataset);
                results.Add(testResult);
            }
        }

        return results.ToArray();
    }

    private static void PrintHeader(PairOfMixtures[] mixPairs, string[] titles)
    {
        Console.WriteLine();

        while (titles.Length < 2)
            titles = [..titles, "_"];

        Console.Write($"{titles[0],-COLSIZE_COND}");
        Console.Write($"{titles[1],-COLSIZE_COND}");

        foreach (var mixPair in mixPairs)
        {
            Console.Write($"{mixPair,-COLSIZE_DIST}");
        }
        Console.WriteLine();
    }

    private static void PrintByAlgorithm(TestResult[] results, PrintedInfo target)
    {
        var list = new Dictionary<Algorithm, List<string>>();
        foreach (var testResult in results)
        {
            if (!list.TryGetValue(testResult.Algorithm, out List<string>? items))
            {
                items = [];
                list[testResult.Algorithm] = items;
            }

            string s = $"{testResult.Algorithm.Name,-COLSIZE_COND}";
            s += $"{testResult.Options,-COLSIZE_COND}";

            if (target == PrintedInfo.Mean)
                foreach (var comparison in testResult.Comparisons)
                    s += $"{comparison.Mean,-COLSIZE_DIST:F4}";
            else
                foreach (var comparison in testResult.Comparisons)
                    s += $"{comparison.Std,-COLSIZE_DIST:F4}";

            items.Add(s);
        }

        Console.WriteLine(target);
        foreach (var (_, lines) in list)
            foreach (var line in lines)
                Console.WriteLine(line);
    }

    private static void PrintByOption(TestResult[] results, PrintedInfo target)
    {
        var list = new Dictionary<Options, List<string>>();
        foreach (var testResult in results)
        {
            if (!list.TryGetValue(testResult.Options, out List<string>? items))
            {
                items = [];
                list[testResult.Options] = items;
            }

            string s = $"{testResult.Options,-COLSIZE_COND}";
            s += $"{testResult.Algorithm.Name,-COLSIZE_COND}";

            if (target == PrintedInfo.Mean)
                foreach (var comparison in testResult.Comparisons)
                    s += $"{comparison.Mean,-COLSIZE_DIST:F4}";
            else
                foreach (var comparison in testResult.Comparisons)
                    s += $"{comparison.Std,-COLSIZE_DIST:F4}";

            items.Add(s);
        }

        Console.WriteLine(target);
        foreach (var (_, lines) in list)
            foreach (var line in lines)
                Console.WriteLine(line);
    }

    record Condition(Algorithm Algorithm, Options Options);

    private static void PrintSuspiciousPairs(TestResult[] results)
    {
        Console.WriteLine();
        Console.WriteLine("Are there any odd results?");

        Array.Sort(results, (a, b) =>
        {
            int result = a.Algorithm.Name.CompareTo(b.Algorithm.Name);
            return result != 0 ? result : a.Options.ToString().CompareTo(b.Options.ToString());
        });
        
        var list = new List<string>();

        foreach (var testResult in results)
        {
            var sameMixtures = testResult.Comparisons.Where(cmps => cmps.Mixtures.IsSame);
            var diffMixtures = testResult.Comparisons.Where(cmps => !cmps.Mixtures.IsSame);

            int totalCount = 0;
            int count = 0;
            double minDistance = double.MaxValue;
            double avgDistanceAtMin = 0;

            foreach (var sameMixResult in sameMixtures)
                foreach (var diffMixResult in diffMixtures)
                    foreach (var smrDist in sameMixResult.Distances)
                        foreach (var dmrDist in diffMixResult.Distances)
                        {
                            totalCount++;
                            var dist = dmrDist - smrDist;
                            if (minDistance > dist)
                                minDistance = dist;
                            if (dmrDist <= smrDist)
                            {
                                count++;
                                avgDistanceAtMin += (dmrDist + smrDist) / 2;
                            }
                        }

            var oddPerc = 100.0 * count / totalCount;
            var threshold = avgDistanceAtMin / (count > 0 ? count : 1);
            string s = $"{testResult.Algorithm.Name,-COLSIZE_COND} {testResult.Options,-COLSIZE_COND} {oddPerc,-COLSIZE_DIST:F2} {minDistance,-COLSIZE_DIST:F4} {threshold,-COLSIZE_DIST:F4}";
            list.Add(s);
        }

        foreach (var item in list)
            Console.WriteLine(item);
    }
}