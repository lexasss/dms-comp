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

    public static bool LoadTwoDmsFiles(Action<Dms?, Dms?> proceed)
    {
        (string? filename1, string? filename2) = SelectTwoDmsFiles();
        if (!string.IsNullOrEmpty(filename1) && !string.IsNullOrEmpty(filename2))
        {
            var dms1 = Dms.Load(filename1);
            var dms2 = Dms.Load(filename2);

            proceed(dms1, dms2);

            return dms1 != null && dms2 != null;
        }
        else if (!string.IsNullOrEmpty(filename1) && string.IsNullOrEmpty(filename2))
        {
            var dmses = Dms.LoadMultiple(filename1);
            if (dmses != null)
            {
                if (dmses.Length > 2)
                {
                    var dialog = new SelectDmsRecord(dmses);
                    if (dialog.ShowDialog() == true)
                    {
                        var selected = dialog.DmsItems.Where(item => item.IsSelected).ToArray();
                        if (selected.Count() >= 2)
                        {
                            proceed(selected[0].Dms, selected[1].Dms);
                            return true;
                        }
                    }
                }
                else if (dmses.Length == 2)
                {
                    proceed(dmses[0], dmses[1]);
                    return true;
                }
                else
                {
                    MessageBox.Show("The file is either corrupted, or has no DMS data, or insufficient number of DMS data records", "DMS data loader", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        return false;
    }

    public static string? SelectDmsFile()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON files|*.json"
        };
        if (ofd.ShowDialog() == true)
        {
            return ofd.FileName;
        }

        return null;
    }

    public static (string?, string?) SelectTwoDmsFiles()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON files|*.json",
            Multiselect = true,
            Title = "Select two DMS JSON files, or one JSON file with multiple DMS records"
        };
        if (ofd.ShowDialog() == true)
        {
            if (ofd.FileNames.Length == 2)
            {
                return (ofd.FileNames[0], ofd.FileNames[1]);
            }
            else if (ofd.FileNames.Length == 1)
            {
                return (ofd.FileNames[0], null);
            }
            else
            {
                MessageBox.Show("Please select no more than two DMS files.", "DMS comparison", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        return (null, null);
    }
}
