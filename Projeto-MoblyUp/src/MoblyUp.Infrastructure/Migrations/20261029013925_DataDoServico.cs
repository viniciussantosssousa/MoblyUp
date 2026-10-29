using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoblyUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DataDoServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DataDesejada",
                table: "PedidosMontagem",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataProposta",
                table: "PedidosMontagem",
                type: "date",
                nullable: true);

            // Pedidos criados antes da data existir: data desejada = dia da criação (horário de Brasília)
            // e, se já houver proposta, o montador "concordou" com essa data
            migrationBuilder.Sql(@"
                UPDATE PedidosMontagem
                SET DataDesejada = CAST(DATEADD(HOUR, -3, DataCriacao) AS date)
                WHERE DataDesejada = '0001-01-01';

                UPDATE PedidosMontagem
                SET DataProposta = DataDesejada
                WHERE ValorProposto IS NOT NULL AND DataProposta IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataDesejada",
                table: "PedidosMontagem");

            migrationBuilder.DropColumn(
                name: "DataProposta",
                table: "PedidosMontagem");
        }
    }
}
