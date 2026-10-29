using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MoblyUp.API.Autenticacao;
using MoblyUp.API.TempoReal;
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

// MODO DE TESTE: só em Development e com "LoginDeTeste:Habilitado": true no appsettings.Development.json
var loginDeTesteHabilitado = builder.Environment.IsDevelopment()
    && builder.Configuration.GetValue<bool>("LoginDeTeste:Habilitado");
const string EsquemaPadrao = "FirebaseOuTeste";

var autenticacao = builder.Services.AddAuthentication(
    loginDeTesteHabilitado ? EsquemaPadrao : JwtBearerDefaults.AuthenticationScheme);

if (loginDeTesteHabilitado)
{
    autenticacao
        .AddScheme<AuthenticationSchemeOptions, LoginDeTesteHandler>(LoginDeTeste.Esquema, null)
        // Com o cabeçalho X-Usuario-Teste usa o login de teste; sem ele, o token do Firebase normalmente
        .AddPolicyScheme(EsquemaPadrao, EsquemaPadrao, options =>
            options.ForwardDefaultSelector = context =>
                LoginDeTeste.ObterUsuario(context.Request) != null
                    ? LoginDeTeste.Esquema
                    : JwtBearerDefaults.AuthenticationScheme);
}

autenticacao
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

        // Tempo real: o WebSocket do navegador não envia cabeçalhos, então na rota do hub
        // o SignalR manda o token na URL (?access_token=...). Só é aceito em /hubs.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = token;
                return Task.CompletedTask;
            },
        };
    });
builder.Services.AddAuthorization();

// Tempo real (SignalR): telas atualizam sozinhas quando um pedido muda
builder.Services.AddSignalR();
builder.Services.AddSingleton<ITempoReal, SignalRTempoReal>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IPedidoMontagemRepository, PedidoMontagemRepository>();
builder.Services.AddScoped<IMensagemChatRepository, MensagemChatRepository>();

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
builder.Services.AddScoped<PerfilPublicoUseCase>();
builder.Services.AddScoped<ChatUseCase>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (loginDeTesteHabilitado)
{
    await LoginDeTeste.CriarUsuariosAsync(app.Services);
    app.Logger.LogWarning("MODO DE TESTE ATIVO: login sem Firebase liberado para os usuários de teste. Desligue em appsettings.Development.json (LoginDeTeste:Habilitado).");
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
app.MapHub<PedidosHub>("/hubs/pedidos");

app.Run();