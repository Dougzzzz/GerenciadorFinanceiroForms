using System;
using System.Threading;
using ControleFinanceiroForms.Features.ImportTransactions;
using ControleFinanceiroForms.Features.Investments;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public sealed class ViewInitializationTests
{
    [TestMethod]
    public void Views_ShouldInitializeWithoutXamlExceptions()
    {
        Exception? initializationException = null;

        // Instanciar controles WPF requer uma thread STA (Single-Threaded Apartment).
        var thread = new Thread(() =>
        {
            try
            {
                // Se algum StaticResource ou erro de binding/namespace estiver quebrado no XAML,
                // o InitializeComponent() chamado pelo construtor irá lançar uma exceção (geralmente XamlParseException).
                _ = new InvestmentsView();
                _ = new ImportTransactionsView();
            }
            catch (Exception ex)
            {
                initializationException = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (initializationException != null)
        {
            Assert.Fail($"A inicialização da View falhou. Isso indica um erro no XAML (ex: StaticResource não encontrado).\nExceção: {initializationException.Message}\nExceção Interna: {initializationException.InnerException?.Message}");
        }
    }
}
