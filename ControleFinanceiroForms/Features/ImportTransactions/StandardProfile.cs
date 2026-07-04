using System.Globalization;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.ImportTransactions;

public class StandardProfile : ICsvProfile
{
    public bool CanHandle(string headerLine)
    {
        return headerLine.Contains("Data", StringComparison.OrdinalIgnoreCase) &&
               headerLine.Contains("Descricao", StringComparison.OrdinalIgnoreCase) &&
               headerLine.Contains("Valor", StringComparison.OrdinalIgnoreCase) &&
               headerLine.Contains("Categoria", StringComparison.OrdinalIgnoreCase);
    }

    public char GetSeparator(string headerLine) => headerLine.Contains(';') ? ';' : ',';

    public Transacao ParseLine(string[] columns, Guid contaId)
    {
        if (columns.Length < 4) throw new FormatException("Invalid column length for StandardProfile");

        string[] dateFormats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" };
        DateTime date = DateTime.ParseExact(columns[0].Trim(), dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);

        string description = columns[1].Trim();
        decimal amount = ParseMonetary(columns[2].Trim());
        string category = columns[3].Trim();

        var tx = Transacao.Create(date, description, amount, null, contaId);
        if (!string.IsNullOrWhiteSpace(category))
        {
            tx.NomeCategoriaOriginal = category;
        }
        return tx;
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
