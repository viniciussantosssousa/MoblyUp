using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoblyUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DataPropostaAceita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataPropostaAceita",
                table: "PedidosMontagem",
                type: "datetime2",
                nullable: true);

            // Pedidos que já passaram do aceite (aguardando pagamento, em andamento, concluídos)
            // e cancelados que chegaram a ter conversa continuam com o chat disponível
            migrationBuilder.Sql(@"
                UPDATE PedidosMontagem
                SET DataPropostaAceita = COALESCE(DataAceite, DataCriacao)
                WHERE Status IN (3, 4, 5)
                   OR (Status = 6 AND EXISTS (SELECT 1 FROM MensagensChat m WHERE m.PedidoId = PedidosMontagem.Id))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataPropostaAceita",
                table: "PedidosMontagem");
        }
    }
}
