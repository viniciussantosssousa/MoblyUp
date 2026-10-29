using MoblyUp.Application.DTOs;
using MoblyUp.Application.Interfaces;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Domain.ValueObjects;

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

        // Avaliações recebidas no papel informado (mesma regra do repositório real)
        private IEnumerable<(string UsuarioId, int Nota, string? Comentario, DateTime Data, string Autor, string Movel)> Notas(TipoPerfil papel) =>
            papel == TipoPerfil.Montador
                ? Pedidos.Where(p => p.MontadorId != null && p.NotaMontador.HasValue)
                    .Select(p => (p.MontadorId!, p.NotaMontador!.Value, p.ComentarioMontador, p.DataAvaliacaoMontador!.Value, p.ClienteId, p.TipoMovel))
                : Pedidos.Where(p => p.NotaCliente.HasValue)
                    .Select(p => (p.ClienteId, p.NotaCliente!.Value, p.ComentarioCliente, p.DataAvaliacaoCliente!.Value, p.MontadorId ?? "", p.TipoMovel));

        public Task<IReadOnlyList<AvaliacaoRecebida>> ObterAvaliacoesRecebidasAsync(string usuarioId, TipoPerfil papel) =>
            Task.FromResult<IReadOnlyList<AvaliacaoRecebida>>(Notas(papel)
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.Data)
                .Select(n => new AvaliacaoRecebida(n.Nota, n.Comentario, n.Data, n.Autor, n.Movel))
                .ToList());

        public Task<Dictionary<string, ResumoReputacao>> ObterResumosReputacaoAsync(IEnumerable<string> usuarioIds, TipoPerfil papel)
        {
            var ids = usuarioIds.ToHashSet();
            return Task.FromResult(Notas(papel)
                .Where(n => ids.Contains(n.UsuarioId))
                .GroupBy(n => n.UsuarioId)
                .ToDictionary(g => g.Key, g => new ResumoReputacao(g.Average(n => n.Nota), g.Count())));
        }

        public Task<bool> PossuemPedidoEmComumAsync(string a, string b) =>
            Task.FromResult(Pedidos.Any(p => (p.ClienteId == a && p.MontadorId == b) || (p.ClienteId == b && p.MontadorId == a)));

        public Task<bool> ClienteTemPedidoAbertoAsync(string clienteId) =>
            Task.FromResult(Pedidos.Any(p => p.ClienteId == clienteId && p.Status == StatusPedido.Disponivel));
    }

    // Guarda as notificações "enviadas" para os testes conferirem
    public class NotificacaoServiceFake : INotificacaoService
    {
        public List<(string UsuarioId, string Titulo)> Enviadas { get; } = new();
        public List<(string UsuarioId, string Titulo, string? Tipo)> EnviadasComTipo { get; } = new();
        public List<(string Endereco, string Titulo)> ParaRegiao { get; } = new();

        public Task NotificarUsuarioAsync(string? usuarioId, string titulo, string mensagem, Guid pedidoId, string? tipo = null)
        {
            if (usuarioId != null)
            {
                Enviadas.Add((usuarioId, titulo));
                EnviadasComTipo.Add((usuarioId, titulo, tipo));
            }
            return Task.CompletedTask;
        }

        public Task NotificarMontadoresDaRegiaoAsync(string enderecoServico, string titulo, string mensagem, Guid pedidoId)
        {
            ParaRegiao.Add((enderecoServico, titulo));
            return Task.CompletedTask;
        }
    }

    public class MensagemChatRepositoryFake : IMensagemChatRepository
    {
        public List<MensagemChat> Mensagens { get; } = new();

        public Task AdicionarAsync(MensagemChat mensagem) { Mensagens.Add(mensagem); return Task.CompletedTask; }

        public Task<IReadOnlyList<MensagemChat>> ObterDoPedidoAsync(Guid pedidoId) =>
            Task.FromResult<IReadOnlyList<MensagemChat>>(Mensagens.Where(m => m.PedidoId == pedidoId).OrderBy(m => m.DataEnvio).ToList());

        public Task<IReadOnlyList<string>> ObterTextosRecentesAsync(Guid pedidoId, string remetenteId, TimeSpan janela, int limite) =>
            Task.FromResult<IReadOnlyList<string>>(Mensagens
                .Where(m => m.PedidoId == pedidoId && m.RemetenteId == remetenteId && m.DataEnvio >= DateTime.UtcNow - janela)
                .OrderByDescending(m => m.DataEnvio).ThenByDescending(m => Mensagens.IndexOf(m))
                .Take(limite).Select(m => m.Texto).ToList());

        public Task<int> MarcarComoLidasAsync(Guid pedidoId, string leitorId)
        {
            var naoLidas = Mensagens.Where(m => m.PedidoId == pedidoId && m.RemetenteId != leitorId && m.DataLeitura == null).ToList();
            naoLidas.ForEach(m => m.MarcarComoLida());
            return Task.FromResult(naoLidas.Count);
        }

        public Task<Dictionary<Guid, int>> ContarNaoLidasAsync(string usuarioId, IEnumerable<Guid> pedidoIds)
        {
            var ids = pedidoIds.ToHashSet();
            return Task.FromResult(Mensagens
                .Where(m => ids.Contains(m.PedidoId) && m.RemetenteId != usuarioId && m.DataLeitura == null)
                .GroupBy(m => m.PedidoId)
                .ToDictionary(g => g.Key, g => g.Count()));
        }
    }

    // Guarda os eventos de tempo real para os testes conferirem
    public class TempoRealFake : ITempoReal
    {
        public List<(IEnumerable<string> Usuarios, string Texto)> MensagensChat { get; } = new();
        public List<(string UsuarioId, Guid PedidoId)> Leituras { get; } = new();

        public Task PedidoAlteradoAsync(Guid pedidoId, string? regiao, IEnumerable<string?> usuarios) => Task.CompletedTask;
        public Task AvisarUsuarioAsync(string usuarioId, string titulo, string mensagem, Guid pedidoId, string? tipo = null) => Task.CompletedTask;
        public Task AvisarRegiaoAsync(string regiao, string titulo, string mensagem, Guid pedidoId) => Task.CompletedTask;

        public Task MensagemChatAsync(IEnumerable<string> usuarios, MensagemChatResponse mensagem)
        {
            MensagensChat.Add((usuarios.ToList(), mensagem.Texto));
            return Task.CompletedTask;
        }

        public Task ChatLidoAsync(string usuarioId, Guid pedidoId)
        {
            Leituras.Add((usuarioId, pedidoId));
            return Task.CompletedTask;
        }
    }

    public static class Cenario
    {
        public const string Endereco = "Rua A, 10, Centro - Uberaba - MG";

        // Data padrão dos pedidos de teste: daqui a 3 dias (sempre no futuro)
        public static DateOnly Data => DataServico.Hoje().AddDays(3);

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
            new(clienteId, "Guarda-roupa", "6 portas", null, Endereco, Data, 2.1m, 1.8m, 0.55m);
    }
}
