using System.Globalization;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.ImportTransactions;

public class DefaultProfile : ICsvProfile
{
    public bool CanHandle(string headerLine)
    {
        return headerLine.Contains("Data", StringComparison.OrdinalIgnoreCase) &&
               headerLine.Contains("Descricao", StringComparison.OrdinalIgnoreCase) &&
               headerLine.Contains("Valor", StringComparison.OrdinalIgnoreCase) &&
               !headerLine.Contains("Nome no Cartão", StringComparison.OrdinalIgnoreCase);
    }

    public char GetSeparator(string headerLine) => headerLine.Contains(';') ? ';' : ',';

    public Transacao ParseLine(string[] columns, AccountType accountType)
    {
        if (columns.Length < 3) throw new FormatException("Invalid column length for DefaultProfile");

        string[] dateFormats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" };
        DateTime date = DateTime.ParseExact(columns[0].Trim(), dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);

        string description = columns[1].Trim();
        decimal amount = ParseMonetary(columns[2].Trim());

        return Transacao.Create(date, description, amount, null, accountType);
    }

    protected decimal ParseMonetary(string amountStr)
    {
        int lastComma = amountStr.LastIndexOf(',');
        int lastDot = amountStr.LastIndexOf('.');

        if (lastComma > lastDot)
            return decimal.Parse(amountStr, NumberStyles.Any, new CultureInfo("pt-BR"));
        else
            return decimal.Parse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture);
    }
}
