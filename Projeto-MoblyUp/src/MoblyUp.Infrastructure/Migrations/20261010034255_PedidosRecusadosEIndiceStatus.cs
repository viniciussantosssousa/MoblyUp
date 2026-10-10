using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoblyUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PedidosRecusadosEIndiceStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PedidosRecusados",
                columns: table => new
                {
                    PedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MontadorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DataRecusa = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosRecusados", x => new { x.PedidoId, x.MontadorId });
                    table.ForeignKey(
                        name: "FK_PedidosRecusados_PedidosMontagem_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "PedidosMontagem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidosRecusados_Usuarios_MontadorId",
                        column: x => x.MontadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PedidosMontagem_Status",
                table: "PedidosMontagem",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosRecusados_MontadorId",
                table: "PedidosRecusados",
                column: "MontadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PedidosRecusados");

            migrationBuilder.DropIndex(
                name: "IX_PedidosMontagem_Status",
                table: "PedidosMontagem");
        }
    }
}
