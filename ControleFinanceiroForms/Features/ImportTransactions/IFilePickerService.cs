using System.IO;

namespace ControleFinanceiroForms.Features.ImportTransactions;

public interface IFilePickerService
{
    Stream? PickCsvFileStream();
}

public class WindowsFilePickerService : IFilePickerService
{
    public Stream? PickCsvFileStream()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            DefaultExt = ".csv",
            Filter = "CSV Files (*.csv)|*.csv"
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            return dialog.OpenFile();
        }

        return null;
    }
}
