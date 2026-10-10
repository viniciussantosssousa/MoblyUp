using MoblyUp.Application.Interfaces;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Tests
{
    // Repositórios em memória: testam as regras sem precisar do SQL Server
    public class UsuarioRepositoryFake : IUsuarioRepository
    {
        public List<Usuario> Usuarios { get; } = new();

        public Task<Usuario?> ObterPorIdAsync(string id) => Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));
        public Task<Usuario?> ObterPorCpfAsync(string cpf) => Task.FromResult(Usuarios.FirstOrDefault(u => u.Cpf == cpf));
        public Task<Usuario?> ObterPorEmailAsync(string email) => Task.FromResult(Usuarios.FirstOrDefault(u => u.Email == email));
        public Task<IEnumerable<Usuario>> ObterPorPushTokenAsync(string token) =>
            Task.FromResult(Usuarios.Where(u => u.ExpoPushToken == token).ToList().AsEnumerable());
        public Task<IEnumerable<Usuario>> ObterMontadoresComPushAsync() =>
            Task.FromResult(Usuarios.Where(u => u.Perfil == TipoPerfil.Montador && u.ExpoPushToken != null).ToList().AsEnumerable());
        public Task AdicionarAsync(Usuario usuario) { Usuarios.Add(usuario); return Task.CompletedTask; }
        public Task AtualizarAsync(Usuario usuario) => Task.CompletedTask;
    }

    public class PedidoRepositoryFake : IPedidoMontagemRepository
    {
        public List<PedidoMontagem> Pedidos { get; } = new();
        public List<(Guid PedidoId, string MontadorId)> Recusas { get; } = new();

        public Task<PedidoMontagem?> ObterPorIdAsync(Guid id) => Task.FromResult(Pedidos.FirstOrDefault(p => p.Id == id));
        public Task<IEnumerable<PedidoMontagem>> ObterPorClienteIdAsync(string clienteId) =>
            Task.FromResult(Pedidos.Where(p => p.ClienteId == clienteId).ToList().AsEnumerable());
        public Task<IEnumerable<PedidoMontagem>> ObterPorMontadorIdAsync(string montadorId) =>
            Task.FromResult(Pedidos.Where(p => p.MontadorId == montadorId).ToList().AsEnumerable());
        public Task<IEnumerable<PedidoMontagem>> ObterPorStatusAsync(StatusPedido status) =>
            Task.FromResult(Pedidos.Where(p => p.Status == status).ToList().AsEnumerable());
        public Task AdicionarAsync(PedidoMontagem pedido) { Pedidos.Add(pedido); return Task.CompletedTask; }
        public Task AtualizarAsync(PedidoMontagem pedido) => Task.CompletedTask;

        public Task RegistrarRecusaAsync(Guid pedidoId, string montadorId)
        {
            if (!Recusas.Contains((pedidoId, montadorId)))
                Recusas.Add((pedidoId, montadorId));
            return Task.CompletedTask;
        }

        public Task<HashSet<Guid>> ObterIdsRecusadosAsync(string montadorId) =>
            Task.FromResult(Recusas.Where(r => r.MontadorId == montadorId).Select(r => r.PedidoId).ToHashSet());
    }

    // Guarda as notificações "enviadas" para os testes conferirem
    public class NotificacaoServiceFake : INotificacaoService
    {
        public List<(string UsuarioId, string Titulo)> Enviadas { get; } = new();
        public List<(string Endereco, string Titulo)> ParaRegiao { get; } = new();

        public Task NotificarUsuarioAsync(string? usuarioId, string titulo, string mensagem, Guid pedidoId)
        {
            if (usuarioId != null) Enviadas.Add((usuarioId, titulo));
            return Task.CompletedTask;
        }

        public Task NotificarMontadoresDaRegiaoAsync(string enderecoServico, string titulo, string mensagem, Guid pedidoId)
        {
            ParaRegiao.Add((enderecoServico, titulo));
            return Task.CompletedTask;
        }
    }

    public static class Cenario
    {
        public const string Endereco = "Rua A, 10, Centro - Uberaba - MG";

        public static Usuario Cliente(string id = "cliente-1", bool comCpf = true) =>
            new("Cliente", comCpf ? GerarCpf(id) : null, $"{id}@teste.com", "", "", TipoPerfil.Cliente, id);

        public static Usuario Montador(string id = "montador-1", bool comPix = true, bool comCpf = true)
        {
            var montador = new Usuario("Montador", comCpf ? GerarCpf(id) : null, $"{id}@teste.com", "", "", TipoPerfil.Montador, id);
            if (comPix) montador.DefinirDadosBancarios($"{id}@pix.com");
            return montador;
        }

        // Gera um CPF válido e estável a partir de um texto (cada usuário de teste tem o seu)
        public static string GerarCpf(string semente)
        {
            var hash = (uint)semente.Aggregate(17, (acc, c) => acc * 31 + c);
            var digitos = (100_000_000 + hash % 800_000_000).ToString().Select(c => c - '0').ToList();

            for (var tamanho = 9; tamanho <= 10; tamanho++)
            {
                var soma = 0;
                for (var i = 0; i < tamanho; i++) soma += digitos[i] * (tamanho + 1 - i);
                var resto = soma % 11;
                digitos.Add(resto < 2 ? 0 : 11 - resto);
            }

            return string.Concat(digitos);
        }

        public static PedidoMontagem Pedido(string clienteId = "cliente-1") =>
            new(clienteId, "Guarda-roupa", "6 portas", null, Endereco);
    }
}
