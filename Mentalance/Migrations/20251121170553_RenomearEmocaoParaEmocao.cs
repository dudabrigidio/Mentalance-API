using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mentalance.Migrations
{
    /// <inheritdoc />
    public partial class RenomearEmocaoParaEmocao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Emoção",
                table: "Checkin",
                newName: "Emocao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Emocao",
                table: "Checkin",
                newName: "Emoção");
        }
    }
}
