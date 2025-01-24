using System.Windows;

namespace DmsComparison;

public class DmsItem(Dms dms, string name, string info, bool isSelected)
{
    public Dms Dms => dms;
    public string Name => name;
    public string Info => info;
    public bool IsSelected { get; set; } = isSelected;
}

public partial class SelectDmsRecord : Window
{
    public DmsItem[] DmsItems { get; }

    public SelectDmsRecord(Dms[] dmses)
    {
        InitializeComponent();

        DmsItems = dmses.Select(dms => {
            string info = "";
            if (!string.IsNullOrEmpty(dms.Info))
            {
                info += $" - {dms.Info}";
            }
            if (dms.Pulses != null)
            {
                info += $" - Pulses {string.Join(", ", dms.Pulses)}";
            }
            return new DmsItem(dms, $"{dms.Date} {dms.Time}", info, false);
        }).ToArray();

        DataContext = this;
    }

    // Internal

    private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        foreach (DmsItem item in e.RemovedItems)
        {
            item.IsSelected = false;
        }

        foreach (DmsItem item in e.AddedItems)
        {
            item.IsSelected = true;
        }

        btnOK.IsEnabled = DmsItems.Where(item => item.IsSelected).Count() == 2;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
