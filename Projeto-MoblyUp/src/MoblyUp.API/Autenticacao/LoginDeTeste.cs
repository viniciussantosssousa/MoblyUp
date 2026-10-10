using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.API.Autenticacao
{
    // MODO DE TESTE (somente desenvolvimento): permite usar o app sem login no Firebase.
    // Só é registrado com ASPNETCORE_ENVIRONMENT=Development e "LoginDeTeste:Habilitado": true
    // (appsettings.Development.json). Aceita apenas os usuários de teste abaixo.
    public static class LoginDeTeste
    {
        public const string Esquema = "LoginDeTeste";
        public const string Cabecalho = "X-Usuario-Teste";

        public static readonly UsuarioDeTeste[] Usuarios =
        {
            new("teste-cliente", "Cliente de Teste", "cliente.teste@moblyup.dev", TipoPerfil.Cliente, "38491027505", null),
            new("teste-montador", "Montador de Teste", "montador.teste@moblyup.dev", TipoPerfil.Montador, "62719304840", "montador.teste@pix.dev"),
            new("teste-montador-2", "Segundo Montador", "montador2.teste@moblyup.dev", TipoPerfil.Montador, "71502836408", "montador2.teste@pix.dev"),
        };

        public const string Regiao = "Uberaba - MG";

        // Cria os usuários de teste no banco se ainda não existirem
        public static async Task CriarUsuariosAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var repositorio = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

            foreach (var teste in Usuarios)
            {
                if (await repositorio.ObterPorIdAsync(teste.Id) != null)
                    continue;

                var usuario = new Usuario(teste.Nome, teste.Cpf, teste.Email, "", "", teste.Perfil, teste.Id);
                usuario.DefinirRegiao(Regiao);
                if (teste.ChavePix != null)
                    usuario.DefinirDadosBancarios(teste.ChavePix);

                await repositorio.AdicionarAsync(usuario);
            }
        }
    }

    public record UsuarioDeTeste(string Id, string Nome, string Email, TipoPerfil Perfil, string Cpf, string? ChavePix);

    // Autentica pelo cabeçalho X-Usuario-Teste em vez do token do Firebase
    public class LoginDeTesteHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public LoginDeTesteHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var id = Request.Headers[LoginDeTeste.Cabecalho].ToString();
            var usuario = LoginDeTeste.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null)
                return Task.FromResult(AuthenticateResult.Fail("Usuário de teste desconhecido."));

            // Mesmas claims que o token do Firebase: "sub" (UID) e "email"
            var identidade = new ClaimsIdentity(new[]
            {
                new Claim("sub", usuario.Id),
                new Claim("email", usuario.Email),
            }, LoginDeTeste.Esquema);

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identidade), LoginDeTeste.Esquema);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
