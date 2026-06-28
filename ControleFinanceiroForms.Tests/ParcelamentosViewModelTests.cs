using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiroForms.Data.Entities;
using ControleFinanceiroForms.Features.Parcelamentos;
using ControleFinanceiroForms.Tests.Fakes;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public class ParcelamentosViewModelTests
{
    private FakeParcelamentoRepository _parcelamentoRepo = null!;
    private FakePagamentoParcelamentoRepository _pagamentoRepo = null!;
    private ParcelamentosViewModel _viewModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _parcelamentoRepo = new FakeParcelamentoRepository();
        _pagamentoRepo = new FakePagamentoParcelamentoRepository();
        _viewModel = new ParcelamentosViewModel(_parcelamentoRepo, _pagamentoRepo);
    }

    // ── ParcelamentoViewModel (Wrapper) Tests ──────────────────────

    [TestMethod]
    public void Wrapper_CalculatesPropertiesCorrectly()
    {
        // Arrange
        var p = new Parcelamento
        {
            Descricao = "Notebook",
            ValorTotal = 6000m,
            NumeroParcelas = 12,
            DataInicio = DateTime.Today
        };
        p.Pagamentos.Add(new PagamentoParcelamento { ValorPago = 1000m });
        p.Pagamentos.Add(new PagamentoParcelamento { ValorPago = 500m });

        var wrapper = new ParcelamentoViewModel(p);

        // Assert
        Assert.AreEqual(1500m, wrapper.ValorPago);
        Assert.AreEqual(4500m, wrapper.SaldoRestante);
        Assert.AreEqual(25.0, wrapper.PercentualQuitado);
        Assert.AreEqual("Em aberto", wrapper.StatusLabel);
        Assert.AreEqual(500m, wrapper.ValorMedioParcela);
        
        var expectedProgress = $"R$ {1500m:N2} de R$ {6000m:N2} ({25.0:F1}%)";
        Assert.AreEqual(expectedProgress, wrapper.FormattedProgress);
    }

    [TestMethod]
    public void Wrapper_WhenFullyPaid_StatusIsQuitado()
    {
        // Arrange
        var p = new Parcelamento
        {
            Descricao = "Celular",
            ValorTotal = 2000m,
            DataInicio = DateTime.Today
        };
        p.Pagamentos.Add(new PagamentoParcelamento { ValorPago = 2000m });

        var wrapper = new ParcelamentoViewModel(p);

        // Assert
        Assert.AreEqual(2000m, wrapper.ValorPago);
        Assert.AreEqual(0m, wrapper.SaldoRestante);
        Assert.AreEqual(100.0, wrapper.PercentualQuitado);
        Assert.AreEqual("Quitado", wrapper.StatusLabel);
        Assert.IsNull(wrapper.ValorMedioParcela);
    }

    [TestMethod]
    public void Wrapper_Overpaid_ClampPercentualTo100AndSaldoTo0()
    {
        // Arrange
        var p = new Parcelamento
        {
            Descricao = "Dívida",
            ValorTotal = 1000m,
            DataInicio = DateTime.Today
        };
        p.Pagamentos.Add(new PagamentoParcelamento { ValorPago = 1200m });

        var wrapper = new ParcelamentoViewModel(p);

        // Assert
        Assert.AreEqual(1200m, wrapper.ValorPago);
        Assert.AreEqual(0m, wrapper.SaldoRestante);
        Assert.AreEqual(100.0, wrapper.PercentualQuitado);
        Assert.AreEqual("Quitado", wrapper.StatusLabel);
    }

    // ── ParcelamentosViewModel Tests ───────────────────────────────

    [TestMethod]
    public async Task AddParcelamentoCommand_ValidInput_AddsToCollectionAndRepository()
    {
        // Arrange
        _viewModel.NewDescricao = "Notebook Gamer";
        _viewModel.NewValorTotal = 5000.00m;
        _viewModel.NewNumeroParcelas = 10;
        _viewModel.NewDataInicio = new DateTime(2026, 6, 1);

        // Act
        await _viewModel.AddParcelamentoCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNull(_viewModel.ErrorMessage);
        Assert.AreEqual(1, _viewModel.VisibleParcelamentos.Count);
        Assert.AreEqual("Notebook Gamer", _viewModel.VisibleParcelamentos[0].Descricao);
        Assert.AreEqual(5000.00m, _viewModel.VisibleParcelamentos[0].ValorTotal);
        Assert.AreEqual(10, _viewModel.VisibleParcelamentos[0].NumeroParcelas);

        var repoItems = await _parcelamentoRepo.GetAllAsync();
        Assert.AreEqual(1, repoItems.Count());
        Assert.AreEqual("Notebook Gamer", repoItems.First().Descricao);

        // Fields reset
        Assert.AreEqual(string.Empty, _viewModel.NewDescricao);
        Assert.IsNull(_viewModel.NewValorTotal);
        Assert.IsNull(_viewModel.NewNumeroParcelas);
        Assert.AreEqual(DateTime.Today, _viewModel.NewDataInicio);
    }

    [TestMethod]
    public async Task AddParcelamentoCommand_EmptyDescricao_ShowsErrorAndDoesNotAdd()
    {
        // Arrange
        _viewModel.NewDescricao = "   ";
        _viewModel.NewValorTotal = 100m;

        // Act
        await _viewModel.AddParcelamentoCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNotNull(_viewModel.ErrorMessage);
        Assert.AreEqual("A descrição do parcelamento é obrigatória.", _viewModel.ErrorMessage);
        Assert.AreEqual(0, _viewModel.VisibleParcelamentos.Count);
    }

    [TestMethod]
    public async Task AddParcelamentoCommand_ZeroOrNegativeValue_ShowsErrorAndDoesNotAdd()
    {
        // Arrange
        _viewModel.NewDescricao = "Teste";
        _viewModel.NewValorTotal = 0m;

        // Act
        await _viewModel.AddParcelamentoCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNotNull(_viewModel.ErrorMessage);
        Assert.AreEqual("O valor total deve ser maior que zero.", _viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task AddPagamentoCommand_ValidInput_UpdatesSelectedParcelamento()
    {
        // Arrange
        var p = new Parcelamento { Id = Guid.NewGuid(), Descricao = "Notebook", ValorTotal = 6000m };
        var pVm = new ParcelamentoViewModel(p);
        _viewModel.VisibleParcelamentos.Add(pVm);
        _viewModel.SelectedParcelamento = pVm;

        _viewModel.NewValorPago = 1500m;
        _viewModel.NewPagamentoNota = "Primeira parcela";

        // Act
        await _viewModel.AddPagamentoCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNull(_viewModel.ErrorMessage);
        Assert.AreEqual(1500m, pVm.ValorPago);
        Assert.AreEqual(4500m, pVm.SaldoRestante);
        Assert.AreEqual(25.0, pVm.PercentualQuitado);
        Assert.AreEqual(1, p.Pagamentos.Count);
        Assert.AreEqual("Primeira parcela", p.Pagamentos.First().Nota);
        
        // Observable collection check
        Assert.AreEqual(1, _viewModel.SelectedParcelamentoPagamentos.Count);
        Assert.AreEqual(1500m, _viewModel.SelectedParcelamentoPagamentos[0].ValorPago);

        // Form cleared
        Assert.IsNull(_viewModel.NewValorPago);
        Assert.AreEqual(string.Empty, _viewModel.NewPagamentoNota);
    }

    [TestMethod]
    public void SelectedParcelamentoChange_SynchronizesSelectedParcelamentoPagamentos()
    {
        // Arrange
        var p = new Parcelamento { Id = Guid.NewGuid(), Descricao = "Notebook", ValorTotal = 6000m };
        p.Pagamentos.Add(new PagamentoParcelamento { ValorPago = 500m });
        p.Pagamentos.Add(new PagamentoParcelamento { ValorPago = 1000m });
        var pVm = new ParcelamentoViewModel(p);

        // Act
        _viewModel.SelectedParcelamento = pVm;

        // Assert
        Assert.AreEqual(2, _viewModel.SelectedParcelamentoPagamentos.Count);
        Assert.AreEqual(500m, _viewModel.SelectedParcelamentoPagamentos[0].ValorPago);
        Assert.AreEqual(1000m, _viewModel.SelectedParcelamentoPagamentos[1].ValorPago);

        // Act 2: Deselect
        _viewModel.SelectedParcelamento = null;

        // Assert 2
        Assert.AreEqual(0, _viewModel.SelectedParcelamentoPagamentos.Count);
    }

    [TestMethod]
    public async Task AddPagamentoCommand_NoSelectedParcelamento_ShowsError()
    {
        // Arrange
        _viewModel.SelectedParcelamento = null;
        _viewModel.NewValorPago = 100m;

        // Act
        await _viewModel.AddPagamentoCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNotNull(_viewModel.ErrorMessage);
        Assert.AreEqual("Selecione um parcelamento para registrar o pagamento.", _viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task AddPagamentoCommand_NegativeValue_ShowsError()
    {
        // Arrange
        var p = new Parcelamento { Id = Guid.NewGuid(), Descricao = "Notebook", ValorTotal = 6000m };
        var pVm = new ParcelamentoViewModel(p);
        _viewModel.SelectedParcelamento = pVm;
        _viewModel.NewValorPago = -100m;

        // Act
        await _viewModel.AddPagamentoCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNotNull(_viewModel.ErrorMessage);
        Assert.AreEqual("O valor pago deve ser maior que zero.", _viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task LoadParcelamentosAsync_LoadsFromRepositoryAndAppliesFilter()
    {
        // Arrange
        var p1 = new Parcelamento { Id = Guid.NewGuid(), Descricao = "Notebook", ValorTotal = 6000m };
        var p2 = new Parcelamento { Id = Guid.NewGuid(), Descricao = "Quitado", ValorTotal = 1000m };
        p2.Pagamentos.Add(new PagamentoParcelamento { ValorPago = 1000m });

        await _parcelamentoRepo.AddAsync(p1);
        await _parcelamentoRepo.AddAsync(p2);

        // Act & Assert 1: Sem filtro
        _viewModel.FiltroSomenteEmAberto = false;
        await _viewModel.LoadParcelamentosCommand.ExecuteAsync(null);
        Assert.AreEqual(2, _viewModel.VisibleParcelamentos.Count);

        // Act & Assert 2: Com filtro ativo
        _viewModel.FiltroSomenteEmAberto = true; // should automatically trigger OnFiltroSomenteEmAbertoChanged and ApplyFilter
        Assert.AreEqual(1, _viewModel.VisibleParcelamentos.Count);
        Assert.AreEqual("Notebook", _viewModel.VisibleParcelamentos[0].Descricao);
    }

    [TestMethod]
    public async Task DeleteParcelamentoCommand_RemovesFromCollectionAndRepository()
    {
        // Arrange
        var p = new Parcelamento { Id = Guid.NewGuid(), Descricao = "A Excluir", ValorTotal = 1000m };
        var pVm = new ParcelamentoViewModel(p);
        await _parcelamentoRepo.AddAsync(p);
        
        await _viewModel.LoadParcelamentosCommand.ExecuteAsync(null);
        Assert.AreEqual(1, _viewModel.VisibleParcelamentos.Count);

        // Act
        await _viewModel.DeleteParcelamentoCommand.ExecuteAsync(_viewModel.VisibleParcelamentos[0]);

        // Assert
        Assert.AreEqual(0, _viewModel.VisibleParcelamentos.Count);
        var repoItems = await _parcelamentoRepo.GetAllAsync();
        Assert.AreEqual(0, repoItems.Count());
    }
}
