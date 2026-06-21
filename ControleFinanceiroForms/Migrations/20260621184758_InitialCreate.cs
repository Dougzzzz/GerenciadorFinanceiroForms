using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiroForms.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BudgetLimit = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Investimentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalValue = table.Column<decimal>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investimentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetasGasto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetasGasto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetasGasto_Categorias_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ChaveExclusiva = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transacoes_Categorias_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetasGasto_CategoryId",
                table: "MetasGasto",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_CategoryId",
                table: "Transacoes",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ChaveExclusiva",
                table: "Transacoes",
                column: "ChaveExclusiva",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Investimentos");

            migrationBuilder.DropTable(
                name: "MetasGasto");

            migrationBuilder.DropTable(
                name: "Transacoes");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
