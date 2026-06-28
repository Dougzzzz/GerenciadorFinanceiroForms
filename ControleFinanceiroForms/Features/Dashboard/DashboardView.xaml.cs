using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ControleFinanceiroForms.Features.Dashboard;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }
}

public class StatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isOverBudget && isOverBudget)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e02020"));
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7132f5"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
