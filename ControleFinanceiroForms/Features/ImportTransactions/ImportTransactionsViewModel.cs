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
    private readonly IPdfParserService _pdfParserService;
    private readonly IFilePickerService _filePickerService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IContaRepository _contaRepository;

    [ObservableProperty]
    private ObservableCollection<Categoria> _categories = new();

    [ObservableProperty]
    private Categoria? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<Conta> _availableContas = new();

    [ObservableProperty]
    private Conta? _selectedConta;

    [ObservableProperty]
    private ObservableCollection<Transacao> _importedTransactions = new();

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isImporting;

    public ImportTransactionsViewModel(
        ITransactionRepository transactionRepository,
        ICsvParserService parserService,
        IPdfParserService pdfParserService,
        IFilePickerService filePickerService,
        ICategoryRepository categoryRepository,
        IContaRepository contaRepository)
    {
        _transactionRepository = transactionRepository;
        _parserService = parserService;
        _pdfParserService = pdfParserService;
        _filePickerService = filePickerService;
        _categoryRepository = categoryRepository;
        _contaRepository = contaRepository;
    }

    [RelayCommand]
    private async Task LoadInitialDataAsync()
    {
        var categoryItems = await _categoryRepository.GetAllAsync();
        Categories.Clear();
        foreach (var item in categoryItems)
        {
            Categories.Add(item);
        }

        var contaItems = await _contaRepository.GetAllAsync();
        AvailableContas.Clear();
        foreach (var item in contaItems)
        {
            AvailableContas.Add(item);
        }

        var txItems = await _transactionRepository.GetAllAsync();
        ImportedTransactions.Clear();
        foreach (var item in txItems.OrderByDescending(t => t.Date).Take(100))
        {
            ImportedTransactions.Add(item);
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        using var stream = _filePickerService.PickFileStream(out string fileExtension);
        if (stream == null)
            return;

        IsImporting = true;
        StatusMessage = "Importando...";

        try
        {
            IEnumerable<Transacao> parsedTransactions;
            var contaId = SelectedConta?.Id ?? Guid.Empty;

            if (fileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                parsedTransactions = await _pdfParserService.ParsePdfAsync(stream);
            }
            else
            {
                parsedTransactions = await _parserService.ParseCsvAsync(stream, contaId);
            }

            var parsedTransactionsList = parsedTransactions.ToList();
            var incomingHashes = parsedTransactionsList.Select(t => t.ChaveExclusiva).Distinct().ToList();
            
            var existingHashesList = await _transactionRepository.GetExistingHashesAsync(incomingHashes);
            var existingHashes = new HashSet<string>(existingHashesList);

            int addedCount = 0;
            int duplicateCount = 0;

            var toAdd = new List<Transacao>();

            foreach (var tx in parsedTransactionsList)
            {
                if (SelectedCategory != null)
                {
                    tx.CategoryId = SelectedCategory.Id;
                }
                else if (!string.IsNullOrWhiteSpace(tx.NomeCategoriaOriginal))
                {
                    var catName = tx.NomeCategoriaOriginal.Trim();
                    var existingCategory = Categories.FirstOrDefault(c => c.Name.Equals(catName, StringComparison.OrdinalIgnoreCase));
                    if (existingCategory != null)
                    {
                        tx.CategoryId = existingCategory.Id;
                    }
                    else
                    {
                        // Create category on the fly
                        var newCategory = new Categoria { Name = catName };
                        await _categoryRepository.AddAsync(newCategory);
                        Categories.Add(newCategory);
                        tx.CategoryId = newCategory.Id;
                    }
                }
                
                // If the parser didn't already set ContaId (e.g. PDF parser), set it here
                if (tx.ContaId == Guid.Empty && SelectedConta != null)
                {
                    tx.ContaId = SelectedConta.Id;
                }

                // Recalculate hash because ContaId or CategoryId may have changed
                tx.GerarHash();

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
                await LoadInitialDataAsync(); // Refresh list
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

    [RelayCommand]
    private async Task DownloadTemplateAsync()
    {
        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "modelo_importacao.csv");
        var csvContent = "Data,Descricao,Valor,Categoria\n2023-12-01,Compra no Mercado,-150.50,Alimentacao\n2023-12-05,Salario,3500.00,Renda\n";
        
        try
        {
            await File.WriteAllTextAsync(filePath, csvContent);
            StatusMessage = $"Modelo salvo em: {filePath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao salvar modelo: {ex.Message}";
        }
    }
}
