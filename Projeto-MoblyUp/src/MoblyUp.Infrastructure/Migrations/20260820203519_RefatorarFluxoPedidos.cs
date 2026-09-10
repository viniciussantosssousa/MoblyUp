using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MoblyUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefatorarFluxoPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidosMontagem_Servicos_ServicoId",
                table: "PedidosMontagem");

            migrationBuilder.DropIndex(
                name: "IX_PedidosMontagem_ServicoId",
                table: "PedidosMontagem");

            migrationBuilder.DeleteData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DropColumn(
                name: "DataEnvioFoto",
                table: "PedidosMontagem");

            migrationBuilder.DropColumn(
                name: "ServicoId",
                table: "PedidosMontagem");

            migrationBuilder.DropColumn(
                name: "ValorTotal",
                table: "PedidosMontagem");

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorMontador",
                table: "PedidosMontagem",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxaPlataforma",
                table: "PedidosMontagem",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "FotoConclusaoUrl",
                table: "PedidosMontagem",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TamanhoMovel",
                table: "PedidosMontagem",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoMovel",
                table: "PedidosMontagem",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UrlFotoMovelTemporaria",
                table: "PedidosMontagem",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorProposto",
                table: "PedidosMontagem",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TamanhoMovel",
                table: "PedidosMontagem");

            migrationBuilder.DropColumn(
                name: "TipoMovel",
                table: "PedidosMontagem");

            migrationBuilder.DropColumn(
                name: "UrlFotoMovelTemporaria",
                table: "PedidosMontagem");

            migrationBuilder.DropColumn(
                name: "ValorProposto",
                table: "PedidosMontagem");

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorMontador",
                table: "PedidosMontagem",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxaPlataforma",
                table: "PedidosMontagem",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FotoConclusaoUrl",
                table: "PedidosMontagem",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEnvioFoto",
                table: "PedidosMontagem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServicoId",
                table: "PedidosMontagem",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "ValorTotal",
                table: "PedidosMontagem",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Ativo", "Nome" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), true, "Móveis de Quarto" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), true, "Móveis de Sala" }
                });

            migrationBuilder.InsertData(
                table: "Servicos",
                columns: new[] { "Id", "Ativo", "CategoriaId", "Descricao", "Nome", "Valor" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), true, new Guid("11111111-1111-1111-1111-111111111111"), "Montagem completa de guarda-roupas de 2 a 6 portas.", "Montagem de Guarda-Roupa", 150.00m },
                    { new Guid("44444444-4444-4444-4444-444444444444"), true, new Guid("22222222-2222-2222-2222-222222222222"), "Montagem e fixação de painel na parede para TVs até 65 polegadas.", "Montagem de Painel de TV", 90.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PedidosMontagem_ServicoId",
                table: "PedidosMontagem",
                column: "ServicoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidosMontagem_Servicos_ServicoId",
                table: "PedidosMontagem",
                column: "ServicoId",
                principalTable: "Servicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
