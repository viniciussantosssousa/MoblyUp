using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Infrastructure.Data;
using MoblyUp.Infrastructure.Repositories;
using MoblyUp.Application.UseCases;
using MoblyUp.Application.Interfaces;
using MoblyUp.Infrastructure.Services;

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
    options.UseSqlServer(builder.Configuration.GetConnectionString("MoblyUpDb")));

// Autenticação: o app envia o ID token do Firebase (Authorization: Bearer ...).
// A API valida a assinatura com as chaves públicas do Google e extrai o UID do usuário.
var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
        options.MapInboundClaims = false; // mantém "sub" como o UID do Firebase
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true,
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IPedidoMontagemRepository, PedidoMontagemRepository>();

// Notificações push via Expo (o envio nunca bloqueia o pedido por mais de 10s)
builder.Services.AddHttpClient<INotificacaoService, ExpoNotificacaoService>(client =>
    client.Timeout = TimeSpan.FromSeconds(10));

builder.Services.AddScoped<GerenciarPerfilUseCase>();
builder.Services.AddScoped<CriarPedidoMontagemUseCase>();
builder.Services.AddScoped<AceitarPedidoUseCase>();
builder.Services.AddScoped<EnviarFotoConclusaoUseCase>();
builder.Services.AddScoped<ConcluirPedidoUseCase>();
builder.Services.AddScoped<ConsultarPedidosUseCase>();
builder.Services.AddScoped<ResponderPropostaUseCase>();
builder.Services.AddScoped<AcoesMontadorUseCase>();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();