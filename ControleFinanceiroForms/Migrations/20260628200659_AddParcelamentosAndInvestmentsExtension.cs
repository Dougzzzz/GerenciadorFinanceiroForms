using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiroForms.Migrations
{
    /// <inheritdoc />
    public partial class AddParcelamentosAndInvestmentsExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Conta",
                table: "Investimentos",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoInvestimento",
                table: "Investimentos",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoOperacao",
                table: "Investimentos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Parcelamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    NumeroParcelas = table.Column<int>(type: "INTEGER", nullable: true),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parcelamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PagamentosParcelamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParcelamentoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValorPago = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Nota = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosParcelamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagamentosParcelamento_Parcelamentos_ParcelamentoId",
                        column: x => x.ParcelamentoId,
                        principalTable: "Parcelamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosParcelamento_ParcelamentoId",
                table: "PagamentosParcelamento",
                column: "ParcelamentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagamentosParcelamento");

            migrationBuilder.DropTable(
                name: "Parcelamentos");

            migrationBuilder.DropColumn(
                name: "Conta",
                table: "Investimentos");

            migrationBuilder.DropColumn(
                name: "TipoInvestimento",
                table: "Investimentos");

            migrationBuilder.DropColumn(
                name: "TipoOperacao",
                table: "Investimentos");
        }
    }
}
