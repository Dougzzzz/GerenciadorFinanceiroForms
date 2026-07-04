using System.Globalization;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.ImportTransactions;

public class FaturaProfile : ICsvProfile
{
    public bool CanHandle(string headerLine)
    {
        return headerLine.Contains("Data de Compra", StringComparison.OrdinalIgnoreCase) &&
               headerLine.Contains("Nome no Cartão", StringComparison.OrdinalIgnoreCase) &&
               headerLine.Contains("Valor (em R$)", StringComparison.OrdinalIgnoreCase);
    }

    public char GetSeparator(string headerLine) => ';';

    public Transacao ParseLine(string[] columns, Guid contaId)
    {
        if (columns.Length < 9) throw new FormatException("Invalid column length for FaturaProfile");

        string[] dateFormats = { "dd/MM/yyyy", "yyyy-MM-dd" };
        DateTime date = DateTime.ParseExact(columns[0].Trim(), dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);

        string description = columns[4].Trim();
        decimal amount = ParseMonetary(columns[8].Trim());
        
        // Em faturas de cartão de crédito, os lançamentos de gastos geralmente vêm positivos,
        // mas no nosso sistema queremos tratar como despesa (negativo).
        if (amount > 0)
            amount = -amount;

        return Transacao.Create(date, description, amount, null, contaId);
    }

    private decimal ParseMonetary(string amountStr)
    {
        int lastComma = amountStr.LastIndexOf(',');
        int lastDot = amountStr.LastIndexOf('.');

        if (lastComma > lastDot)
            return decimal.Parse(amountStr, NumberStyles.Any, new CultureInfo("pt-BR"));
        else
            return decimal.Parse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture);
    }
}
