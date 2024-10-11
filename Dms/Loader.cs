using System.Windows;

namespace DmsComparison;

public static class Loader
{
    public static bool LoadDmsFile(Action<Dms?> proceed)
    {
        string? filename = SelectDmsFile();
        if (!string.IsNullOrEmpty(filename))
        {
            var dms = Dms.Load(filename);
            proceed(dms);

            return dms != null;
        }

        return false;
    }

    public static bool LoadTwoDmsFiles(Action<Dms?> proceed1, Action<Dms?> proceed2)
    {
        (string? filename1, string? filename2) = SelectTwoDmsFiles();
        if (!string.IsNullOrEmpty(filename1) && !string.IsNullOrEmpty(filename2))
        {
            var dms1 = Dms.Load(filename1);
            proceed1(dms1);

            var dms2 = Dms.Load(filename2);
            proceed2(dms2);

            return dms1 != null && dms2 != null;
        }

        return false;
    }

    public static string? SelectDmsFile()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog();
        ofd.Filter = "JSON files|*.json";
        if (ofd.ShowDialog() == true)
        {
            return ofd.FileName;
        }

        return null;
    }

    public static (string?, string?) SelectTwoDmsFiles()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog();
        ofd.Filter = "JSON files|*.json";
        ofd.Multiselect = true;
        ofd.Title = "Select two DMS files";
        if (ofd.ShowDialog() == true)
        {
            if (ofd.FileNames.Length == 2)
            {
                return (ofd.FileNames[0], ofd.FileNames[1]);
            }
            else
            {
                MessageBox.Show("Please select exactly two DMS files.", "DMS comparison", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        return (null, null);
    }
}
