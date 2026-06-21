using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleFinanceiroForms.Data;
using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.ImportTransactions;

public partial class ImportTransactionsViewModel : ObservableObject
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICsvParserService _parserService;
    private readonly IFilePickerService _filePickerService;
    private readonly ICategoryRepository _categoryRepository;

    [ObservableProperty]
    private ObservableCollection<Categoria> _categories = new();

    [ObservableProperty]
    private Categoria? _selectedCategory;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isImporting;

    public ImportTransactionsViewModel(
        ITransactionRepository transactionRepository,
        ICsvParserService parserService,
        IFilePickerService filePickerService,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _parserService = parserService;
        _filePickerService = filePickerService;
        _categoryRepository = categoryRepository;
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        var items = await _categoryRepository.GetAllAsync();
        Categories.Clear();
        foreach (var item in items)
        {
            Categories.Add(item);
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        using var stream = _filePickerService.PickCsvFileStream();
        if (stream == null)
            return;

        IsImporting = true;
        StatusMessage = "Importando...";

        try
        {
            var parsedTransactions = await _parserService.ParseCsvAsync(stream);

            var existingTransactions = await _transactionRepository.GetAllAsync();
            var existingHashes = new HashSet<string>(existingTransactions.Select(t => t.ChaveExclusiva));

            int addedCount = 0;
            int duplicateCount = 0;

            var toAdd = new List<Transacao>();

            foreach (var tx in parsedTransactions)
            {
                if (SelectedCategory != null)
                {
                    tx.CategoryId = SelectedCategory.Id;
                }

                if (existingHashes.Contains(tx.ChaveExclusiva))
                {
                    duplicateCount++;
                }
                else
                {
                    toAdd.Add(tx);
                    existingHashes.Add(tx.ChaveExclusiva); // Prevent duplicates within the same file
                    addedCount++;
                }
            }

            if (toAdd.Any())
            {
                await _transactionRepository.AddRangeAsync(toAdd);
            }

            StatusMessage = $"Importação concluída. Adicionadas: {addedCount}. Duplicadas ignoradas: {duplicateCount}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro na importação: {ex.Message}";
        }
        finally
        {
            IsImporting = false;
        }
    }
}
