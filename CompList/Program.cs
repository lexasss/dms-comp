/* Computes distances between odor mixtures collected in Naa's setup using all available algorithms and conditions at once.
 * 
 * The script reads all JSON files in the selected folder.
 * There should be 18 JSON files in Naa's setup, but the script may take any amount of files.
 * Also, each JSON must contain a comment { text: "..." } with the first word labelling the measurement category.
 * In Naa's setup, these were Mix17, Mix50 and Mix83 produced with 2ch AutOlD "Comparison" utility.
 * However, other labels should be OK also.
 * 
 * The scripts outputs 3 tables into console: mean, std and the analysis of anomalies.
 * Each table should be copied to the Excel sheet.
 * It also copies an Excel template to the data root folder where "filter-on" and "filter-off" folders are located,
 * and fills the corresponding Excel sheet with the output values.
 */

namespace CompList;

using DmsComparison;
using DmsComparison.Algorithms;
//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Data;
using System.Data.OleDb;

class PairOfMixtures(string mix1, string mix2)
{
    public string Id => $"{mix1}/{mix2}";
    public bool AreSame => mix1 == mix2;
    public override string ToString() => Id;
}
record MixtureDatasets(string MixType, HashSet<Dms> DmsSet);
record ComparisonResult(PairOfMixtures Mixtures, double Mean, double Std, double[] Distances);
record TestResult(Options Options, Algorithm Algorithm, ComparisonResult[] Comparisons);

enum PrintTarget
{
    Mean,
    Std
}

public class Program
{
    static bool VERBOSE => false;
    static bool DEBUG => true;

    const int COLSIZE_COND = 20;
    const int COLSIZE_DIST = 9;

    static bool OUTPUT_TO_EXCEL => false;
    static bool USE_OLE_EXCEL => true;
    const string EXCEL_FILENAME = "analysis.xlsx";
    const int EXCEL_FIRST_ROW = 4;

    public static void Main()
    {
        var (folder, files) = SelectFolder();
        if (folder == null || files == null)
            return;

        string? excelFileName = null;
        string sheetName = "";
        
        if (OUTPUT_TO_EXCEL)
        {
            (excelFileName, sheetName) = CreateExcelFile(folder);
        }

        var datasets = LoadData(files);

        if (VERBOSE)
        {
            Console.WriteLine($"\nSets:");
            foreach (var (mix, list) in datasets)
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

        var results = Run(options, algorithms, datasets);

        var headers = results.First().Comparisons.Select(c => c.Mixtures).ToArray();

        //PrintHeader(headers, ["Options", "Algorithm"]);
        //PrintByOption(results, PrintTarget.Mean);
        //PrintByOption(results, PrintTarget.Std);

        OleDbConnection? excel = null;
        //SpreadsheetDocument? excel2 = null;

        if (OUTPUT_TO_EXCEL)
        {
            if (USE_OLE_EXCEL)
            {
                string connectionSetup = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={excelFileName};Extended Properties=""Excel 12.0 Xml;HDR=NO"";";
                excel = new OleDbConnection(connectionSetup);
                excel.Open();

                EnsureExcelSheetExists(excel, sheetName);
            }
            else
            {
                /*
                excel2 = SpreadsheetDocument.Open(excelFileName ?? "", true);
                var sheet = GetSheet(excel2, sheetName);
                excel2.Close();
                */
            }
        }

        PrintHeader(headers, ["Algorithms", "Options"]);
        PrintByAlgorithm(results, PrintTarget.Mean, excel, sheetName);
        PrintByAlgorithm(results, PrintTarget.Std, excel, sheetName);

        PrintExceptionStats(results, excel, sheetName);

        excel?.Close();
    }

    // Preparion methods

    private static (string, string) CreateExcelFile(string folder)
    {
        var path = folder.Split(Path.DirectorySeparatorChar);
        var excelFileNameAtDataLocation = string.Join(Path.DirectorySeparatorChar, [.. path[0..^1], EXCEL_FILENAME]);

        if (!File.Exists(excelFileNameAtDataLocation))
        {
            File.Copy(EXCEL_FILENAME, excelFileNameAtDataLocation);
        }

        //Process.Start("explorer", "\"" + excelFileNameAtDataLocation + "\"");

        var sheetName = path[^1];

        return (excelFileNameAtDataLocation, sheetName);
    }

    /// <summary>
    /// Suppose to create a new sheet it does not exist, but currently DOES NOT WORK
    /// </summary>
    /// <param name="excel">Excel document</param>
    /// <param name="sheetName">sheet name to check, and also to create if it does not exits</param>
    /// <returns>True if the sheet was in the Excel document already</returns>
    private static bool EnsureExcelSheetExists(OleDbConnection excel, string sheetName)
    {
        var sheets = excel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
        if (sheets == null)
            return false;

        bool exists = false;
        foreach (DataRow row in sheets.Rows)
        {
            var tableName = row["TABLE_NAME"].ToString()?.Trim('\'', '$');
            if (tableName == sheetName)
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            var command = excel.CreateCommand();
            command.CommandText = $"CREATE TABLE {sheetName}";
            command.ExecuteNonQuery();
        }

        return exists;
    }

    /*
    private static SheetData? GetSheet(SpreadsheetDocument? excel, string sheetName)
    {
        if (excel == null)
            return null;

        Sheets? sheets = excel.WorkbookPart?.Workbook.GetFirstChild<Sheets>();
        if (sheets == null)
            return null;

        SheetData? result = null;
        foreach (Sheet sheet in sheets)
        {
            var tableName = sheet.Name?.Value ?? "";
            if (tableName == sheetName)
            {
                var wsps = excel.WorkbookPart.WorksheetParts;
                foreach (var wsp in wsps)
                {
                    var s = wsp.Worksheet.ChildElements.FirstOrDefault(el => el is SheetData) as SheetData;
                    if (s != null)
                    {
                        result = s;
                        break;
                    }
                }
                break;
            }
        }

        if (result == null)
        {
            WorksheetPart? newWorksheetPart = excel.WorkbookPart?.AddNewPart<WorksheetPart>();
            if (newWorksheetPart != null)
            {
                result = new SheetData();
                newWorksheetPart.Worksheet = new Worksheet(result);

                string relationshipId = excel.WorkbookPart?.GetIdOfPart(newWorksheetPart) ?? "";

                uint sheetId = 1;
                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId?.Value ?? 0).Max() + 1;
                }

                Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
                sheets.Append(sheet);
            }
        }

        return result;
    }*/

    private static (string?, string[]?) SelectFolder()
    {
        var ofd = new OpenFolder.FolderPicker
        {
            OkButtonLabel = "Load",
            ForceFileSystem = false,
            Title = "Select a folder with DMS data"
        };
        if (ofd.ShowDialog() == false || string.IsNullOrEmpty(ofd.ResultPath))
        {
            return (null, null);
        }

        var folder = ofd.ResultPath;
        var files = Directory.EnumerateFiles(folder, "*.json");

        Console.WriteLine($"DMS source folder: {folder}");
        Console.WriteLine($"DMS files: {files.Count()}");

        return (folder, files.ToArray());
    }

    private static MixtureDatasets[] LoadData(string[] files)
    {
        var result = new Dictionary<string, HashSet<Dms>>();
        foreach (var file in files)
        {
            var dms = Dms.Load(file);
            string? mixType = dms?.MixType;
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

        return result.Select(kv => new MixtureDatasets(kv.Key, kv.Value)).ToArray();
    }

    private static Algorithm[] LoadAlgorithms()
    {
        var result = new List<Algorithm>();
        var algorithmTypes = Algorithm.GetDescendantTypes();
        foreach (var algorithmType in algorithmTypes)
        {
            if (Activator.CreateInstance(algorithmType) is Algorithm algorithm)
            {
                if (!algorithm.IsVisible || (DEBUG && algorithm.Name != "Canberra"))
                    continue;
                result.Add(algorithm);
            }
        }

        return result.ToArray();
    }

    private static Options[] PermutateOptions()
    {
        var booleans = new bool[] { false, true };
        var result = new List<Options>();
        foreach (var crop in booleans)
            foreach (var norm in Enum.GetValues<Normalization.Type>())
                foreach (var rect in booleans)
                    result.Add(new Options(rect, norm, crop));
        return result.ToArray();
    }

    // Distance estimation methods

    private static TestResult Test(
        Options options,
        Algorithm algorithm,
        MixtureDatasets[] allMixDatasets)
    {
        Console.Write($"{algorithm.Name,-15}");
        if (VERBOSE)
            Console.WriteLine();

        var results = new List<ComparisonResult>();

        var mixTypes = allMixDatasets.Select(md => md.MixType);
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

                var dmsSet1 = allMixDatasets.First(md => md.MixType == mixType1).DmsSet.ToArray();
                var dmsSet2 = allMixDatasets.First(md => md.MixType == mixType2).DmsSet.ToArray();

                for (int i = 0; i < dmsSet1.Length; i++)
                {
                    var jStart = mixType1 == mixType2 ? i + 1 : 0;
                    for (int j = jStart; j < dmsSet2.Length; j++)
                    {
                        var dms1 = dmsSet1[i];
                        var dms2 = dmsSet2[j];
                        if (dms1 == dms2 || dms1.Width != dms2.Width || dms1.Height != dms2.Height)
                            continue;

                        distanceCount += 1;
                        var dist = algorithm.ComputeDistance(dms1.Data, dms2.Data, new Size(dms1.Width, dms2.Height), options);
                        distanceSum += dist;
                        distances.Add(dist);

                        if (VERBOSE)
                            Console.WriteLine($"    {dms1.Time} / {dms2.Time}   =>   {dist:F5}");
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
            Console.WriteLine($"\nOptions: {options}");
            foreach (var algorithm in algorithms)
            {
                var testResult = Test(options, algorithm, mixDataset);
                results.Add(testResult);
            }
        }


        results.Sort((a, b) =>
        {
            int result = a.Algorithm.Name.CompareTo(b.Algorithm.Name);
            if (result != 0)
                return result;
            result = a.Options.Crop.CompareTo(b.Options.Crop);
            if (result != 0)
                return result;
            result = a.Options.NormalizationType.CompareTo(b.Options.NormalizationType);
            if (result != 0)
                return result;
            result = a.Options.UseRectification.CompareTo(b.Options.UseRectification);
            return result;
        });

        return results.ToArray();
    }

    // Output methods

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

    private static void PrintByAlgorithm(TestResult[] results, PrintTarget target, OleDbConnection? excel, string? sheet)
    {
        int excelRow = EXCEL_FIRST_ROW;

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

            OleDbCommand? excelCommand = null;
            if (excel != null)
            {
                var excelCells = "";
                if (target == PrintTarget.Mean)
                    excelCells = $"A{excelRow}:H{excelRow}";
                else if (target == PrintTarget.Std)
                    excelCells = $"M{excelRow}:T{excelRow}";

                excelCommand = new OleDbCommand($"UPDATE [{sheet}${excelCells}] SET F1=@alg, F2=@opt, F3=@d1, F4=@d2, F5=@d3, F6=@d4, F7=@d5, F8=@d6", excel);
                excelCommand.Parameters.AddWithValue("@alg", testResult.Algorithm.Name).OleDbType = OleDbType.Char;
                excelCommand.Parameters.AddWithValue("@opt", testResult.Options.ToString()).OleDbType = OleDbType.Char;
            }

            var index = 1;
            if (target == PrintTarget.Mean)
            {
                foreach (var comparison in testResult.Comparisons)
                {
                    s += $"{comparison.Mean,-COLSIZE_DIST:F5}";
                    if (excelCommand != null)
                        excelCommand.Parameters.AddWithValue($"@d{index++}", (float)comparison.Mean).OleDbType = OleDbType.Decimal;
                }
            }
            else if (target == PrintTarget.Std)
            {
                foreach (var comparison in testResult.Comparisons)
                {
                    s += $"{comparison.Std,-COLSIZE_DIST:F5}";
                    if (excelCommand != null)
                        excelCommand.Parameters.AddWithValue($"@d{index++}", (float)comparison.Std).OleDbType = OleDbType.Decimal;
                }
            }

            items.Add(s);

            if (excelCommand != null)
            {
                excelCommand.ExecuteNonQuery();
                excelRow++;
            }
        }

        Console.WriteLine(target);
        foreach (var (_, lines) in list)
            foreach (var line in lines)
                Console.WriteLine(line);
    }

    private static void PrintByOption(TestResult[] results, PrintTarget target, OleDbConnection? excel, string sheet)
    {
        int excelRow = EXCEL_FIRST_ROW;

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

            OleDbCommand? excelCommand = null;
            if (excel != null)
            {
                var excelCells = "";
                if (target == PrintTarget.Mean)
                    excelCells = $"A{excelRow}:H{excelRow}";
                else if (target == PrintTarget.Std)
                    excelCells = $"M{excelRow}:T{excelRow}";

                excelCommand = new OleDbCommand($"UPDATE [{sheet}${excelCells}] SET F1=@alg, F2=@opt, F3=@d1, F4=@d2, F5=@d3, F6=@d4, F7=@d5, F8=@d6", excel);
                excelCommand.Parameters.AddWithValue("@alg", testResult.Algorithm.Name).OleDbType = OleDbType.Char;
                excelCommand.Parameters.AddWithValue("@opt", testResult.Options.ToString()).OleDbType = OleDbType.Char;
            }

            var index = 1;
            if (target == PrintTarget.Mean)
            {
                foreach (var comparison in testResult.Comparisons)
                {
                    s += $"{comparison.Mean,-COLSIZE_DIST:F5}";
                    if (excelCommand != null)
                        excelCommand.Parameters.AddWithValue($"@d{index++}", comparison.Mean).OleDbType = OleDbType.Decimal;
                }
            }
            else if (target == PrintTarget.Std)
            {
                foreach (var comparison in testResult.Comparisons)
                {
                    s += $"{comparison.Std,-COLSIZE_DIST:F5}";
                    if (excelCommand != null)
                        excelCommand.Parameters.AddWithValue($"@d{index++}", comparison.Std).OleDbType = OleDbType.Decimal;
                }
            }

            items.Add(s);

            if (excelCommand != null)
            {
                excelCommand.ExecuteNonQuery();
                excelRow++;
            }
        }

        Console.WriteLine(target);
        foreach (var (_, lines) in list)
            foreach (var line in lines)
                Console.WriteLine(line);
    }

    private static void PrintExceptionStats(TestResult[] results, OleDbConnection? excel, string sheet)
    {
        Console.WriteLine();
        Console.WriteLine("Exception analysis");

        int excelRow = EXCEL_FIRST_ROW;

        var list = new List<string>();

        foreach (var testResult in results)
        {
            var sameMixtures = testResult.Comparisons.Where(cmps => cmps.Mixtures.AreSame);
            var diffMixtures = testResult.Comparisons.Where(cmps => !cmps.Mixtures.AreSame);

            int totalCount = 0;
            int count = 0;
            double minDistance = double.MaxValue;

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
                                count++;
                        }

            var oddPerc = 100.0 * count / totalCount;
            var (threshold, successRate) = EstimateThreshold(sameMixtures.ToArray(), diffMixtures.ToArray());
            string s = $"{testResult.Algorithm.Name,-COLSIZE_COND} {testResult.Options,-COLSIZE_COND} {oddPerc,-COLSIZE_DIST:F2} {minDistance,-COLSIZE_DIST:F5} {threshold,-COLSIZE_DIST:F5} {successRate,-COLSIZE_DIST:F5}";
            list.Add(s);

            if (excel != null)
            {
                var command = new OleDbCommand($"UPDATE [{sheet}$X{excelRow}:AC{excelRow}] SET F1=@alg, F2=@opt, F3=@d1, F4=@d2, F5=@d3, F6=@d4", excel);
                command.Parameters.AddWithValue("@alg", testResult.Algorithm.Name).OleDbType = OleDbType.Char;
                command.Parameters.AddWithValue("@opt", testResult.Options.ToString()).OleDbType = OleDbType.Char;
                command.Parameters.AddWithValue("@d1", oddPerc).OleDbType = OleDbType.Decimal;
                command.Parameters.AddWithValue("@d2", minDistance).OleDbType = OleDbType.Decimal;
                command.Parameters.AddWithValue("@d3", threshold).OleDbType = OleDbType.Decimal;
                command.Parameters.AddWithValue("@d4", successRate).OleDbType = OleDbType.Decimal;
                command.ExecuteNonQuery();
                excelRow++;
            }
        }

        foreach (var item in list)
            Console.WriteLine(item);
    }

    /// <returns>threshold and average share of the values that stay within the threshold
    /// ("same" - below the threshold, "diff" - above the threshold)</returns>
    private static (double, double) EstimateThreshold(ComparisonResult[] sameMixtures, ComparisonResult[] diffMixtures)
    {
        var distOfSameMixtures = new List<double>();
        foreach (var sameMixResult in sameMixtures)
            distOfSameMixtures.AddRange(sameMixResult.Distances);
        distOfSameMixtures.Sort();
        distOfSameMixtures.Reverse();

        var distOfDiffMixtures = new List<double>();
        foreach (var diffMixResult in diffMixtures)
            distOfDiffMixtures.AddRange(diffMixResult.Distances);
        distOfDiffMixtures.Sort();

        double threshold = 0;
        for (int i = 0; i < distOfDiffMixtures.Count && i < distOfSameMixtures.Count; i++)
            if (distOfDiffMixtures[i] > distOfSameMixtures[i])
            {
                threshold = (distOfDiffMixtures[i] + distOfSameMixtures[i]) / 2;
                break;
            }

        return (threshold,
            (double)(distOfSameMixtures.Where(d => d < threshold).Count() + distOfDiffMixtures.Where(d => d >= threshold).Count()) / 
                (distOfSameMixtures.Count + distOfDiffMixtures.Count));
    }
}