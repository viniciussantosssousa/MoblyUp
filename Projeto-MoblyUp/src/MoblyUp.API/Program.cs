using Microsoft.EntityFrameworkCore;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Infrastructure.Data;
using MoblyUp.Infrastructure.Repositories;
using MoblyUp.Application.UseCases;

var builder = WebApplication.CreateBuilder(args);

// Configuração do CORS para permitir o Front-end
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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
builder.Services.AddScoped<ConsultarPedidosUseCase>();
builder.Services.AddScoped<ResponderPropostaUseCase>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ativando a política de CORS antes do roteamento
app.UseCors("AllowAll");

// Serve as fotos enviadas pelo app (wwwroot/uploads)
app.UseStaticFiles();

// Em desenvolvimento o app no celular acessa a API por HTTP na rede local
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();

app.Run();