using ControleFinanceiroForms.Features.ImportTransactions;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public class CsvParserServiceTests
{
    private CsvParserService _parserService = null!;

    [TestInitialize]
    public void Setup()
    {
        _parserService = new CsvParserService();
    }

    [TestMethod]
    public async Task ParseCsvAsync_ValidStandardCsv_ReturnsParsedTransactions()
    {
        // Arrange
        var csvContent = "Data,Descricao,Valor\n2026-05-10,Supermercado,-250.75\n2026-05-12,Salario,5000.00";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _parserService.ParseCsvAsync(stream);

        // Assert
        Assert.AreEqual(2, result.Count());
        
        var first = result.First();
        Assert.AreEqual(new DateTime(2026, 5, 10), first.Date);
        Assert.AreEqual("Supermercado", first.Description);
        Assert.AreEqual(-250.75m, first.Amount);

        var second = result.Last();
        Assert.AreEqual(new DateTime(2026, 5, 12), second.Date);
        Assert.AreEqual("Salario", second.Description);
        Assert.AreEqual(5000.00m, second.Amount);
    }

    [TestMethod]
    public async Task ParseCsvAsync_CsvWithPtBrCultureDecimals_ReturnsParsedTransactions()
    {
        // Arrange
        // Using comma as decimal separator
        var csvContent = "Data;Descricao;Valor\n10/05/2026;Supermercado;-250,75\n12/05/2026;Salario;5000,00";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _parserService.ParseCsvAsync(stream);

        // Assert
        Assert.AreEqual(2, result.Count());
        Assert.AreEqual(-250.75m, result.First().Amount);
        Assert.AreEqual(5000.00m, result.Last().Amount);
    }

    [TestMethod]
    public async Task ParseCsvAsync_EmptyFile_ReturnsEmptyList()
    {
        // Arrange
        var csvContent = "";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _parserService.ParseCsvAsync(stream);

        // Assert
        Assert.IsFalse(result.Any());
    }

    [TestMethod]
    public async Task ParseCsvAsync_InvalidData_SkipsUnparseableLines()
    {
        // Arrange
        var csvContent = "Data,Descricao,Valor\nNotADate,Supermercado,NotANumber";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

        // Act — invalid lines are now skipped, not thrown (review-002 issue 007)
        var result = await _parserService.ParseCsvAsync(stream);

        // Assert — line was unparseable, so no transactions returned
        Assert.IsFalse(result.Any());
    }
}
