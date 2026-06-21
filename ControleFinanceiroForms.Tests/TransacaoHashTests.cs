using ControleFinanceiroForms.Data.Entities;

namespace ControleFinanceiroForms.Tests;

/// <summary>
/// Unit tests for the <see cref="Transacao.GerarHash"/> deduplication logic.
/// Validates that the SHA-256 hash correctly identifies duplicate and distinct transactions.
/// </summary>
[TestClass]
public sealed class TransacaoHashTests
{
    // ─────────────────────────────────────────────────────────────
    // Unit Tests — GerarHash()
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Two transactions with identical Date, Description, and Amount
    /// must produce the same ChaveExclusiva hash.
    /// </summary>
    [TestMethod]
    public void GerarHash_IdenticalFields_ProducesSameHash()
    {
        // Arrange
        var date = new DateTime(2026, 1, 15);
        const string description = "Supermercado Carrefour";
        const decimal amount = 250.75m;

        var tx1 = Transacao.Create(date, description, amount);
        var tx2 = Transacao.Create(date, description, amount);

        // Assert — Create() calls GerarHash() internally
        Assert.AreEqual(tx1.ChaveExclusiva, tx2.ChaveExclusiva,
            "Identical transactions must produce identical hashes (duplicate detection).");
    }

    /// <summary>
    /// Two transactions that differ only in Amount must produce different hashes.
    /// </summary>
    [TestMethod]
    public void GerarHash_DifferentAmounts_ProducesDifferentHashes()
    {
        // Arrange
        var date = new DateTime(2026, 1, 15);
        const string description = "Restaurante X";

        var tx1 = Transacao.Create(date, description, 100.00m);
        var tx2 = Transacao.Create(date, description, 100.01m);

        // Assert
        Assert.AreNotEqual(tx1.ChaveExclusiva, tx2.ChaveExclusiva,
            "Transactions with different amounts must not collide.");
    }

    /// <summary>
    /// Two transactions that differ only in Description must produce different hashes.
    /// </summary>
    [TestMethod]
    public void GerarHash_DifferentDescriptions_ProducesDifferentHashes()
    {
        // Arrange
        var date = new DateTime(2026, 2, 1);
        const decimal amount = 50.00m;

        var tx1 = Transacao.Create(date, "Farmácia A", amount);
        var tx2 = Transacao.Create(date, "Farmácia B", amount);

        // Assert
        Assert.AreNotEqual(tx1.ChaveExclusiva, tx2.ChaveExclusiva,
            "Transactions with different descriptions must not collide.");
    }

    /// <summary>
    /// Two transactions that differ only in Date must produce different hashes.
    /// </summary>
    [TestMethod]
    public void GerarHash_DifferentDates_ProducesDifferentHashes()
    {
        // Arrange
        const string description = "Netflix";
        const decimal amount = 39.90m;

        var tx1 = Transacao.Create(new DateTime(2026, 3, 1), description, amount);
        var tx2 = Transacao.Create(new DateTime(2026, 4, 1), description, amount);

        // Assert
        Assert.AreNotEqual(tx1.ChaveExclusiva, tx2.ChaveExclusiva,
            "Transactions on different dates must not collide.");
    }

    /// <summary>
    /// The hash must be a 64-character lowercase hex string (SHA-256).
    /// </summary>
    [TestMethod]
    public void GerarHash_ProducesValidSha256HexString()
    {
        // Arrange
        var tx = Transacao.Create(new DateTime(2026, 6, 1), "Test", 1.00m);

        // Assert
        Assert.AreEqual(64, tx.ChaveExclusiva.Length, "SHA-256 hex string must be 64 characters.");
        Assert.IsTrue(tx.ChaveExclusiva.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')),
            "Hash must be lowercase hex.");
    }

    /// <summary>
    /// Calling GerarHash twice on the same object must produce the same result (determinism).
    /// </summary>
    [TestMethod]
    public void GerarHash_IsDeterministic_SameInputSameOutput()
    {
        // Arrange — Create() already calls GerarHash(); capture that first hash.
        var tx = Transacao.Create(new DateTime(2026, 1, 1), "Test determinism", 99.99m);
        var firstHash = tx.ChaveExclusiva;

        // Act — explicitly call GerarHash() again to verify idempotence.
        tx.GerarHash();
        var secondHash = tx.ChaveExclusiva;

        // Assert
        Assert.AreEqual(firstHash, secondHash, "GerarHash must be deterministic.");
    }

    // ─────────────────────────────────────────────────────────────
    // Regression tests — Issue 005 (culture-invariant hash)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// GerarHash must produce the SAME ChaveExclusiva regardless of the
    /// current thread culture. A period is always used as the decimal separator
    /// in the hash input (InvariantCulture).
    /// Regression guard for issue_005 (cross-locale hash inconsistency).
    /// </summary>
    [TestMethod]
    public void GerarHash_IsCultureInvariant_SameHashOnAnyLocale()
    {
        const decimal amount = 1234.50m;
        var date = new DateTime(2026, 1, 1);
        const string description = "Cross-locale test";

        // Compute hash with InvariantCulture thread (default in tests)
        var txInvariant = Transacao.Create(date, description, amount);

        // Compute hash with pt-BR culture active on the thread
        string hashPtBr;
        var originalCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
        try
        {
            System.Threading.Thread.CurrentThread.CurrentCulture =
                new System.Globalization.CultureInfo("pt-BR");
            var txPtBr = Transacao.Create(date, description, amount);
            hashPtBr = txPtBr.ChaveExclusiva;
        }
        finally
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = originalCulture;
        }

        Assert.AreEqual(txInvariant.ChaveExclusiva, hashPtBr,
            "GerarHash must produce identical hashes regardless of thread culture (InvariantCulture required).");
    }
}
