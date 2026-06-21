using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ControleFinanceiroForms.Features.ImportTransactions;

public partial class ImportTransactionsView : UserControl
{
    public ImportTransactionsView()
    {
        InitializeComponent();
    }
}

public class InverseBooleanConverter : IValueConverter
{
    public static InverseBooleanConverter Instance { get; } = new InverseBooleanConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return !boolean;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return !boolean;
        }
        return value;
    }
}
