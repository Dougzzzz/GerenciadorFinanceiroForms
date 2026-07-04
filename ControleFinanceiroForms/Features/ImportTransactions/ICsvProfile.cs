using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Features.ImportTransactions;

public interface ICsvProfile
{
    bool CanHandle(string headerLine);
    char GetSeparator(string headerLine);
    Transacao ParseLine(string[] columns, AccountType accountType);
}
