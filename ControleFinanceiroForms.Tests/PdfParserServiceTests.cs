using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControleFinanceiroForms.Features.ImportTransactions;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public class PdfParserServiceTests
{
    private PdfParserService _parserService = null!;

    [TestInitialize]
    public void Setup()
    {
        _parserService = new PdfParserService();
    }

    private byte[] CreateTestPdfBytes(string[] lines)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var page = builder.AddPage(595, 842);
        
        double y = 800;
        foreach (var line in lines)
        {
            page.AddText(line, 12, new PdfPoint(25, y), font);
            y -= 20; // Mover para baixo para a próxima linha
        }
        
        return builder.Build();
    }

    [TestMethod]
    public async Task ParsePdfAsync_ValidStandardFormat_ReturnsParsedTransactions()
    {
        // Arrange
        var lines = new[]
        {
            "2026-05-10 Supermercado -250.75",
            "2026-05-12 Salario 5000.00"
        };
        var pdfBytes = CreateTestPdfBytes(lines);
        using var stream = new MemoryStream(pdfBytes);

        // Act
        var result = await _parserService.ParsePdfAsync(stream);

        // Assert
        Assert.AreEqual(2, result.Count());
        
        var first = result.First();
        Assert.AreEqual(new System.DateTime(2026, 5, 10), first.Date);
        Assert.AreEqual("Supermercado", first.Description);
        Assert.AreEqual(-250.75m, first.Amount);

        var second = result.Last();
        Assert.AreEqual(new System.DateTime(2026, 5, 12), second.Date);
        Assert.AreEqual("Salario", second.Description);
        Assert.AreEqual(5000.00m, second.Amount);
    }

    [TestMethod]
    public async Task ParsePdfAsync_PtBrCultureAndInvalidLines_ParsesCorrectlyAndIgnoresNoise()
    {
        // Arrange
        var lines = new[]
        {
            "Extrato de Conta Corrente",
            "10/05/2026 Compra no Supermercado -250,75",
            "Saldo Parcial: 1500,00",
            "12/05/2026 Salario Recebido 5000,00",
            "Fim do Extrato"
        };
        var pdfBytes = CreateTestPdfBytes(lines);
        using var stream = new MemoryStream(pdfBytes);

        // Act
        var result = await _parserService.ParsePdfAsync(stream);

        // Assert
        Assert.AreEqual(2, result.Count(), "Deveria filtrar as linhas de ruído e retornar apenas 2 transações.");
        
        var first = result.First();
        Assert.AreEqual(new System.DateTime(2026, 5, 10), first.Date);
        Assert.AreEqual("Compra no Supermercado", first.Description);
        Assert.AreEqual(-250.75m, first.Amount);

        var second = result.Last();
        Assert.AreEqual(new System.DateTime(2026, 5, 12), second.Date);
        Assert.AreEqual("Salario Recebido", second.Description);
        Assert.AreEqual(5000.00m, second.Amount);
    }
}
