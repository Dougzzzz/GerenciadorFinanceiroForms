using System.Globalization;
using System.IO;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.ImportTransactions;

public interface ICsvParserService
{
    Task<IEnumerable<Transacao>> ParseCsvAsync(Stream stream);
}

public class CsvParserService : ICsvParserService
{
    public async Task<IEnumerable<Transacao>> ParseCsvAsync(Stream stream)
    {
        var transactions = new List<Transacao>();
        using var reader = new StreamReader(stream);
        
        string? headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return transactions;
        }

        // Determine separator based on header
        char separator = headerLine.Contains(';') ? ';' : ',';

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = line.Split(separator);
            if (values.Length < 3) continue;

            try
            {
                // Simple heuristic parsing
                // Try different date formats
                string[] dateFormats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" };
                DateTime date = DateTime.ParseExact(values[0].Trim(), dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
                
                string description = values[1].Trim();
                
                // Try different number formats (comma vs dot)
                string amountStr = values[2].Trim();
                decimal amount;
                int lastComma = amountStr.LastIndexOf(',');
                int lastDot = amountStr.LastIndexOf('.');
                
                if (lastComma > lastDot)
                {
                    // Comma is the decimal separator (e.g., pt-BR)
                    amount = decimal.Parse(amountStr, NumberStyles.Any, new CultureInfo("pt-BR"));
                }
                else
                {
                    // Dot is the decimal separator (or no separator) (Invariant)
                    amount = decimal.Parse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture);
                }

                var tx = Transacao.Create(date, description, amount);
                transactions.Add(tx);
            }
            catch (Exception)
            {
                // Skip lines that don't match expected format (review-002 issue 007).
                // Matches PdfParserService behavior — graceful degradation.
                continue;
            }
        }

        return transactions;
    }
}
