using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiroForms.Migrations
{
    /// <inheritdoc />
    public partial class AddContas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contas", x => x.Id);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "ContaId",
                table: "Transacoes",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("INSERT INTO Contas (Id, Name, Type) VALUES ('3b890833-2895-46eb-a4f6-8c4d27da3ff0', 'Conta Corrente Padrão', 0)");
            migrationBuilder.Sql("INSERT INTO Contas (Id, Name, Type) VALUES ('7d812328-98e6-424a-b9c2-5e3be16a5b28', 'Cartão de Crédito Padrão', 1)");
            
            migrationBuilder.Sql("UPDATE Transacoes SET ContaId = '3b890833-2895-46eb-a4f6-8c4d27da3ff0' WHERE AccountType = 0");
            migrationBuilder.Sql("UPDATE Transacoes SET ContaId = '7d812328-98e6-424a-b9c2-5e3be16a5b28' WHERE AccountType = 1");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Transacoes");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ContaId",
                table: "Transacoes",
                column: "ContaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacoes_Contas_ContaId",
                table: "Transacoes",
                column: "ContaId",
                principalTable: "Contas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacoes_Contas_ContaId",
                table: "Transacoes");

            migrationBuilder.DropTable(
                name: "Contas");

            migrationBuilder.DropIndex(
                name: "IX_Transacoes_ContaId",
                table: "Transacoes");

            migrationBuilder.DropColumn(
                name: "ContaId",
                table: "Transacoes");

            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "Transacoes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
