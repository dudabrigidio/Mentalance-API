using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mentalance.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRecomendacaoAnaliseSemanal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "Recomendacao",
                table: "Analiscd mcd m  eSemanal",
                type: "NVARCHAR2(2000)",
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Recomendacao",
                table: "AnaliseSemanal");

        }
    }
}
