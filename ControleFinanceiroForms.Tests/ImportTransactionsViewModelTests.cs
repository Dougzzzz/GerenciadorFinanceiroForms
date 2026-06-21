using System.Collections.ObjectModel;
using ControleFinanceiroForms.Data.Entities;
using ControleFinanceiroForms.Features.ImportTransactions;
using ControleFinanceiroForms.Tests.Fakes;

namespace ControleFinanceiroForms.Tests;

public class FakeFilePickerService : IFilePickerService
{
    public Stream? StreamToReturn { get; set; }
    
    public Stream? PickCsvFileStream()
    {
        return StreamToReturn;
    }
}

public class FakeCsvParserService : ICsvParserService
{
    public IEnumerable<Transacao> TransactionsToReturn { get; set; } = new List<Transacao>();
    
    public Task<IEnumerable<Transacao>> ParseCsvAsync(Stream stream)
    {
        return Task.FromResult(TransactionsToReturn);
    }
}

[TestClass]
public class ImportTransactionsViewModelTests
{
    private FakeTransactionRepository _repository = null!;
    private FakeCsvParserService _parserService = null!;
    private FakeFilePickerService _filePickerService = null!;
    private FakeCategoryRepository _categoryRepository = null!;
    private ImportTransactionsViewModel _viewModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _repository = new FakeTransactionRepository();
        _parserService = new FakeCsvParserService();
        _filePickerService = new FakeFilePickerService();
        _categoryRepository = new FakeCategoryRepository();
        
        _viewModel = new ImportTransactionsViewModel(
            _repository, 
            _parserService, 
            _filePickerService,
            _categoryRepository);
    }

    [TestMethod]
    public async Task ImportCommand_ValidFile_ImportsOnlyNewTransactions()
    {
        // Arrange
        var existingTx = Transacao.Create(new DateTime(2026, 1, 1), "Old Tx", 100);
        await _repository.AddAsync(existingTx);

        var duplicateTx = Transacao.Create(new DateTime(2026, 1, 1), "Old Tx", 100);
        var newTx = Transacao.Create(new DateTime(2026, 1, 2), "New Tx", 200);

        _parserService.TransactionsToReturn = new List<Transacao> { duplicateTx, newTx };
        _filePickerService.StreamToReturn = new MemoryStream();

        // Act
        await _viewModel.ImportCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(2, _repository.Count, "Should only add new transaction, rejecting duplicate.");
        var allTx = await _repository.GetAllAsync();
        Assert.IsTrue(allTx.Any(t => t.Description == "New Tx"));
    }

    [TestMethod]
    public async Task ImportCommand_NoFileSelected_DoesNothing()
    {
        // Arrange
        _filePickerService.StreamToReturn = null;
        _parserService.TransactionsToReturn = new List<Transacao> { Transacao.Create(DateTime.Now, "Tx", 10) };

        // Act
        await _viewModel.ImportCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(0, _repository.Count);
    }
}
