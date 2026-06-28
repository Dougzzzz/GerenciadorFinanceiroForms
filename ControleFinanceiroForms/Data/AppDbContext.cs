using ControleFinanceiroForms.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiroForms.Data;

/// <summary>
/// Entity Framework Core DbContext for the Controle Financeiro application.
/// Manages SQLite persistence for all core entities.
/// </summary>
/// <remarks>
/// Register via DI in <c>App.xaml.cs</c>:
/// <code>
/// services.AddDbContext&lt;AppDbContext&gt;(options =>
///     options.UseSqlite(connectionString)
///            .UseLoggerFactory(loggerFactory)
///            .EnableSensitiveDataLogging(isDevelopment));
/// </code>
/// </remarks>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Transacao> Transacoes => Set<Transacao>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<MetaGasto> MetasGasto => Set<MetaGasto>();
    public DbSet<Investimento> Investimentos => Set<Investimento>();
    public DbSet<Parcelamento> Parcelamentos => Set<Parcelamento>();
    public DbSet<PagamentoParcelamento> PagamentosParcelamento => Set<PagamentoParcelamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Transacao ──────────────────────────────────────────────
        modelBuilder.Entity<Transacao>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Date).IsRequired();
            entity.Property(t => t.Amount).IsRequired().HasColumnType("TEXT"); // SQLite stores decimals as TEXT
            entity.Property(t => t.Description).IsRequired().HasMaxLength(500);
            entity.Property(t => t.ChaveExclusiva).IsRequired().HasMaxLength(64);

            // Unique constraint on deduplication hash
            entity.HasIndex(t => t.ChaveExclusiva).IsUnique();

            entity.HasOne(t => t.Categoria)
                  .WithMany(c => c.Transacoes)
                  .HasForeignKey(t => t.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Categoria ──────────────────────────────────────────────
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.BudgetLimit).HasColumnType("TEXT");
        });

        // ── MetaGasto ──────────────────────────────────────────────
        modelBuilder.Entity<MetaGasto>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.TargetAmount).IsRequired().HasColumnType("TEXT");
            entity.Property(m => m.Month).IsRequired();
            entity.Property(m => m.Year).IsRequired();

            // Enforce one goal per category per month/year to prevent duplicate
            // budget goals that would silently double "Realizado vs. Orçado" totals.
            entity.HasIndex(m => new { m.CategoryId, m.Month, m.Year }).IsUnique();

            entity.HasOne(m => m.Categoria)
                  .WithMany(c => c.MetasGasto)
                  .HasForeignKey(m => m.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Investimento ───────────────────────────────────────────
        modelBuilder.Entity<Investimento>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.RecordedAt).IsRequired();
            entity.Property(i => i.TotalValue).IsRequired().HasColumnType("TEXT");
            entity.Property(i => i.Note).HasMaxLength(500);
            entity.Property(i => i.Conta).HasMaxLength(200);
            entity.Property(i => i.TipoInvestimento).HasMaxLength(200);
            entity.Property(i => i.TipoOperacao)
                  .HasConversion<int>()
                  .HasDefaultValue(OperacaoInvestimento.SnapshotTotal);
        });

        // ── Parcelamento ───────────────────────────────────────────
        modelBuilder.Entity<Parcelamento>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Descricao).IsRequired().HasMaxLength(500);
            entity.Property(p => p.ValorTotal).IsRequired().HasColumnType("TEXT");
            entity.Property(p => p.DataInicio).IsRequired();
            entity.Property(p => p.CriadoEm).IsRequired();

            entity.HasMany(p => p.Pagamentos)
                  .WithOne()
                  .HasForeignKey(pp => pp.ParcelamentoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── PagamentoParcelamento ──────────────────────────────────
        modelBuilder.Entity<PagamentoParcelamento>(entity =>
        {
            entity.HasKey(pp => pp.Id);
            entity.Property(pp => pp.ValorPago).IsRequired().HasColumnType("TEXT");
            entity.Property(pp => pp.DataPagamento).IsRequired();
            entity.Property(pp => pp.Nota).HasMaxLength(500);
        });
    }
}
