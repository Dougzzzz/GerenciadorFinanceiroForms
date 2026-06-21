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

        var tx1 = new Transacao { Date = date, Description = description, Amount = amount };
        var tx2 = new Transacao { Date = date, Description = description, Amount = amount };

        // Act
        tx1.GerarHash();
        tx2.GerarHash();

        // Assert
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

        var tx1 = new Transacao { Date = date, Description = description, Amount = 100.00m };
        var tx2 = new Transacao { Date = date, Description = description, Amount = 100.01m };

        // Act
        tx1.GerarHash();
        tx2.GerarHash();

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

        var tx1 = new Transacao { Date = date, Description = "Farmácia A", Amount = amount };
        var tx2 = new Transacao { Date = date, Description = "Farmácia B", Amount = amount };

        // Act
        tx1.GerarHash();
        tx2.GerarHash();

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

        var tx1 = new Transacao { Date = new DateTime(2026, 3, 1), Description = description, Amount = amount };
        var tx2 = new Transacao { Date = new DateTime(2026, 4, 1), Description = description, Amount = amount };

        // Act
        tx1.GerarHash();
        tx2.GerarHash();

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
        var tx = new Transacao
        {
            Date = new DateTime(2026, 6, 1),
            Description = "Test",
            Amount = 1.00m
        };

        // Act
        tx.GerarHash();

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
        // Arrange
        var tx = new Transacao
        {
            Date = new DateTime(2026, 1, 1),
            Description = "Test determinism",
            Amount = 99.99m
        };

        // Act
        tx.GerarHash();
        var firstHash = tx.ChaveExclusiva;
        tx.GerarHash(); // call again
        var secondHash = tx.ChaveExclusiva;

        // Assert
        Assert.AreEqual(firstHash, secondHash, "GerarHash must be deterministic.");
    }
}
