using System;
using System.Globalization;
using System.Windows.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Converters;

public class EnumTranslationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AccountType accountType)
        {
            return accountType switch
            {
                AccountType.Checking => "Conta Corrente",
                AccountType.CreditCard => "Cartão de Crédito",
                _ => accountType.ToString()
            };
        }
        
        if (value is OperacaoInvestimento operacao)
        {
            return operacao switch
            {
                OperacaoInvestimento.SnapshotTotal => "Consolidado",
                OperacaoInvestimento.Aporte => "Aporte",
                OperacaoInvestimento.Retirada => "Retirada",
                _ => operacao.ToString()
            };
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
