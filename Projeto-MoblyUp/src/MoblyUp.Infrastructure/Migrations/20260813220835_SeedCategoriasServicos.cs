using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MoblyUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategoriasServicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
