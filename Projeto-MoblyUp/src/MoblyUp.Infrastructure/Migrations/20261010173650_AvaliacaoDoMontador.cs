using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoblyUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AvaliacaoDoMontador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvaliacaoComentario",
                table: "PedidosMontagem",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvaliacaoNota",
                table: "PedidosMontagem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAvaliacao",
                table: "PedidosMontagem",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvaliacaoComentario",
                table: "PedidosMontagem");

            migrationBuilder.DropColumn(
                name: "AvaliacaoNota",
                table: "PedidosMontagem");

            migrationBuilder.DropColumn(
                name: "DataAvaliacao",
                table: "PedidosMontagem");
        }
    }
}
