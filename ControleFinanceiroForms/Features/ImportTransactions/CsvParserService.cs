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
                
                if (amountStr.Contains(',') && !amountStr.Contains('.'))
                {
                    // Likely pt-BR format: "1234,56"
                    amount = decimal.Parse(amountStr, new CultureInfo("pt-BR"));
                }
                else
                {
                    // Likely invariant format: "1234.56"
                    amount = decimal.Parse(amountStr, CultureInfo.InvariantCulture);
                }

                var tx = Transacao.Create(date, description, amount);
                transactions.Add(tx);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Failed to parse line: {line}", ex);
            }
        }

        return transactions;
    }
}
