using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiroForms.Migrations
{
    /// <inheritdoc />
    public partial class AddMetaGastoUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MetasGasto_CategoryId",
                table: "MetasGasto");

            migrationBuilder.CreateIndex(
                name: "IX_MetasGasto_CategoryId_Month_Year",
                table: "MetasGasto",
                columns: new[] { "CategoryId", "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MetasGasto_CategoryId_Month_Year",
                table: "MetasGasto");

            migrationBuilder.CreateIndex(
                name: "IX_MetasGasto_CategoryId",
                table: "MetasGasto",
                column: "CategoryId");
        }
    }
}
