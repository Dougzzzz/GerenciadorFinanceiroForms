using System.IO;

namespace ControleFinanceiroForms.Features.ImportTransactions;

public interface IFilePickerService
{
    Stream? PickCsvFileStream();
    Stream? PickPdfFileStream();
    Stream? PickFileStream(out string fileExtension);
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

    public Stream? PickPdfFileStream()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            DefaultExt = ".pdf",
            Filter = "PDF Files (*.pdf)|*.pdf"
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            return dialog.OpenFile();
        }

        return null;
    }

    public Stream? PickFileStream(out string fileExtension)
    {
        fileExtension = string.Empty;
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Arquivos Suportados (*.csv;*.pdf)|*.csv;*.pdf|Arquivos CSV (*.csv)|*.csv|Arquivos PDF (*.pdf)|*.pdf"
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            fileExtension = Path.GetExtension(dialog.FileName).ToLowerInvariant();
            return dialog.OpenFile();
        }

        return null;
    }
}
