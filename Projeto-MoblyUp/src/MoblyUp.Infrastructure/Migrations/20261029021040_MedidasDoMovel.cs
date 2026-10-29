using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoblyUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MedidasDoMovel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AlturaM",
                table: "PedidosMontagem",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ComprimentoM",
                table: "PedidosMontagem",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LarguraM",
                table: "PedidosMontagem",
                type: "decimal(5,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlturaM",
                table: "PedidosMontagem");

            migrationBuilder.DropColumn(
                name: "ComprimentoM",
                table: "PedidosMontagem");

            migrationBuilder.DropColumn(
                name: "LarguraM",
                table: "PedidosMontagem");
        }
    }
}
