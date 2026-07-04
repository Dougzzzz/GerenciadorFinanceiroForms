using System;
using System.Collections.Generic;

namespace ControleFinanceiroForms.Data.Entities;

public enum ContaType
{
    Corrente = 0,
    CartaoDeCredito = 1
}

public class Conta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ContaType Type { get; set; }
    
    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}
