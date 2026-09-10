using Microsoft.EntityFrameworkCore;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Infrastructure.Data;
using MoblyUp.Infrastructure.Repositories;
using MoblyUp.Application.UseCases;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=localhost;Database=MoblyUpDb;Trusted_Connection=True;TrustServerCertificate=True;"));

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IPedidoMontagemRepository, PedidoMontagemRepository>();

builder.Services.AddScoped<CriarContaMontadorUseCase>();
builder.Services.AddScoped<CriarPedidoMontagemUseCase>();
builder.Services.AddScoped<AceitarPedidoUseCase>();
builder.Services.AddScoped<EnviarFotoConclusaoUseCase>();
builder.Services.AddScoped<ConcluirPedidoUseCase>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();