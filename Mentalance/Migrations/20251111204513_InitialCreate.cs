using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mentalance.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnaliseSemanal",
                columns: table => new
                {
                    IdAnalise = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    IdUsuario = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SemanaReferencia = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    EmocaoPredominante = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Resumo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnaliseSemanal", x => x.IdAnalise);
                });

            migrationBuilder.CreateTable(
                name: "Checkin",
                columns: table => new
                {
                    IdCheckin = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    IdUsuario = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DataCheckin = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Emoção = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Texto = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    AnáliseSentimento = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    RespostaGerada = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkin", x => x.IdCheckin);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Senha = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Cargo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.IdUsuario);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnaliseSemanal");

            migrationBuilder.DropTable(
                name: "Checkin");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
