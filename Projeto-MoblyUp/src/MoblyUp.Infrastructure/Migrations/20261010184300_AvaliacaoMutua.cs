using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoblyUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AvaliacaoMutua : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DataAvaliacao",
                table: "PedidosMontagem",
                newName: "DataAvaliacaoMontador");

            migrationBuilder.RenameColumn(
                name: "AvaliacaoNota",
                table: "PedidosMontagem",
                newName: "NotaMontador");

            migrationBuilder.RenameColumn(
                name: "AvaliacaoComentario",
                table: "PedidosMontagem",
                newName: "ComentarioMontador");

            migrationBuilder.AddColumn<string>(
                name: "ComentarioCliente",
                table: "PedidosMontagem",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAvaliacaoCliente",
                table: "PedidosMontagem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotaCliente",
                table: "PedidosMontagem",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComentarioCliente",
                table: "PedidosMontagem");

            migrationBuilder.DropColumn(
                name: "DataAvaliacaoCliente",
                table: "PedidosMontagem");

            migrationBuilder.DropColumn(
                name: "NotaCliente",
                table: "PedidosMontagem");

            migrationBuilder.RenameColumn(
                name: "NotaMontador",
                table: "PedidosMontagem",
                newName: "AvaliacaoNota");

            migrationBuilder.RenameColumn(
                name: "DataAvaliacaoMontador",
                table: "PedidosMontagem",
                newName: "DataAvaliacao");

            migrationBuilder.RenameColumn(
                name: "ComentarioMontador",
                table: "PedidosMontagem",
                newName: "AvaliacaoComentario");
        }
    }
}
