using System.IO;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.ImportTransactions;

public interface ICsvParserService
{
    Task<IEnumerable<Transacao>> ParseCsvAsync(Stream stream, AccountType accountType = AccountType.Checking);
}

public class CsvParserService : ICsvParserService
{
    private readonly IEnumerable<ICsvProfile> _profiles;

    public CsvParserService()
    {
        _profiles = new List<ICsvProfile>
        {
            new FaturaProfile(),
            new DefaultProfile()
        };
    }

    public async Task<IEnumerable<Transacao>> ParseCsvAsync(Stream stream, AccountType accountType = AccountType.Checking)
    {
        var transactions = new List<Transacao>();
        using var reader = new StreamReader(stream);
        
        string? headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return transactions;
        }

        var profile = _profiles.FirstOrDefault(p => p.CanHandle(headerLine));
        if (profile == null)
        {
            profile = new DefaultProfile(); 
        }

        char separator = profile.GetSeparator(headerLine);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = line.Split(separator);

            try
            {
                var tx = profile.ParseLine(values, accountType);
                transactions.Add(tx);
            }
            catch (Exception)
            {
                // Skip lines that don't match expected format
                continue;
            }
        }

        return transactions;
    }
}
