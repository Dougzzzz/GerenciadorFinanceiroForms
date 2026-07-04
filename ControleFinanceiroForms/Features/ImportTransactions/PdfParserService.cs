using System.Globalization;
using System.IO;
using ControleFinanceiroForms.Data.Entities;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ControleFinanceiroForms.Features.ImportTransactions;

public interface IPdfParserService
{
    Task<IEnumerable<Transacao>> ParsePdfAsync(Stream stream);
}

public class PdfParserService : IPdfParserService
{
    public async Task<IEnumerable<Transacao>> ParsePdfAsync(Stream stream)
    {
        var transactions = new List<Transacao>();

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Position = 0;

        using var document = PdfDocument.Open(ms);
        foreach (var page in document.GetPages())
        {
            var words = page.GetWords();
            if (words == null || !words.Any()) continue;

            // Group words by their Y coordinate to reconstruct physical lines
            var linesOfWords = new List<List<Word>>();
            foreach (var word in words)
            {
                var added = false;
                foreach (var lineWords in linesOfWords)
                {
                    var lineY = lineWords[0].BoundingBox.Centroid.Y;
                    if (Math.Abs(word.BoundingBox.Centroid.Y - lineY) < 5)
                    {
                        lineWords.Add(word);
                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    linesOfWords.Add(new List<Word> { word });
                }
            }

            // Sort lines from top to bottom (Y descending)
            var sortedLines = linesOfWords
                .OrderByDescending(lw => lw[0].BoundingBox.Centroid.Y)
                .ToList();

            var textLines = new List<string>();
            foreach (var lineWords in sortedLines)
            {
                // Sort words in the same line from left to right (X ascending)
                var sortedWords = lineWords.OrderBy(w => w.BoundingBox.Left).ToList();
                var lineText = string.Join(" ", sortedWords.Select(w => w.Text));
                textLines.Add(lineText);
            }

            foreach (var line in textLines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                var values = trimmedLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 3) continue;

                // Try to parse the first token as Date
                string[] dateFormats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" };
                if (!DateTime.TryParseExact(values[0].Trim(), dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    continue; // Skip lines that don't start with a valid date
                }

                // The last token should be the amount
                string amountStr = values[^1].Trim();
                decimal amount;
                int lastComma = amountStr.LastIndexOf(',');
                int lastDot = amountStr.LastIndexOf('.');

                try
                {
                    if (lastComma > lastDot)
                    {
                        amount = decimal.Parse(amountStr, NumberStyles.Any, new CultureInfo("pt-BR"));
                    }
                    else
                    {
                        amount = decimal.Parse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture);
                    }
                }
                catch
                {
                    continue; // Skip lines where the last token is not a valid amount
                }

                // The tokens in the middle are the description
                string description = string.Join(" ", values.Skip(1).Take(values.Length - 2)).Trim();
                if (string.IsNullOrWhiteSpace(description))
                {
                    description = "Transação sem descrição";
                }

                var tx = Transacao.Create(date, description, amount, Guid.Empty);
                transactions.Add(tx);
            }
        }

        return transactions;
    }
}
