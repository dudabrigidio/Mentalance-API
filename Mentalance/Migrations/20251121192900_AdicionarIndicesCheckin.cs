using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mentalance.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIndicesCheckin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Checkin_IdUsuario",
                table: "Checkin",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Checkin_IdUsuario_DataCheckin",
                table: "Checkin",
                columns: new[] { "IdUsuario", "DataCheckin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Checkin_IdUsuario",
                table: "Checkin");

            migrationBuilder.DropIndex(
                name: "IX_Checkin_IdUsuario_DataCheckin",
                table: "Checkin");
        }
    }
}
