using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoblyUp.Application.Interfaces;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Domain.ValueObjects;
using MoblyUp.Infrastructure.Data;

namespace MoblyUp.Infrastructure.Repositories
{
    public class PedidoMontagemRepository : IPedidoMontagemRepository
    {
        private readonly AppDbContext _context;
        private readonly ITempoReal _tempoReal;

        public PedidoMontagemRepository(AppDbContext context, ITempoReal tempoReal)
        {
            _context = context;
            _tempoReal = tempoReal;
        }

        public async Task<PedidoMontagem?> ObterPorIdAsync(Guid id)
        {
            return await _context.PedidosMontagem
                .Include(p => p.Cliente)
                .Include(p => p.Montador)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // As listagens são somente leitura: AsNoTracking evita o custo de rastrear as entidades

        public async Task<IEnumerable<PedidoMontagem>> ObterPorClienteIdAsync(string clienteId)
        {
            return await _context.PedidosMontagem
                .AsNoTracking()
                .Include(p => p.Montador)
                .Where(p => p.ClienteId == clienteId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PedidoMontagem>> ObterPorMontadorIdAsync(string montadorId)
        {
            return await _context.PedidosMontagem
                .AsNoTracking()
                .Include(p => p.Cliente)
                .Where(p => p.MontadorId == montadorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PedidoMontagem>> ObterPorStatusAsync(StatusPedido status)
        {
            return await _context.PedidosMontagem
                .AsNoTracking()
                .Include(p => p.Cliente)
                .Where(p => p.Status == status)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<AvaliacaoRecebida>> ObterAvaliacoesRecebidasAsync(string usuarioId, TipoPerfil papel)
        {
            var consulta = _context.PedidosMontagem.AsNoTracking();

            var avaliacoes = papel == TipoPerfil.Montador
                // Montador recebe a nota que o cliente deu
                ? await consulta
                    .Where(p => p.MontadorId == usuarioId && p.NotaMontador != null)
                    .Select(p => new AvaliacaoRecebida(p.NotaMontador!.Value, p.ComentarioMontador,
                        p.DataAvaliacaoMontador!.Value, p.Cliente.Nome, p.TipoMovel))
                    .ToListAsync()
                // Cliente recebe a nota que o montador deu
                : await consulta
                    .Where(p => p.ClienteId == usuarioId && p.NotaCliente != null)
                    .Select(p => new AvaliacaoRecebida(p.NotaCliente!.Value, p.ComentarioCliente,
                        p.DataAvaliacaoCliente!.Value, p.Montador!.Nome, p.TipoMovel))
                    .ToListAsync();

            return avaliacoes.OrderByDescending(a => a.Data).ToList();
        }

        public async Task<Dictionary<string, ResumoReputacao>> ObterResumosReputacaoAsync(IEnumerable<string> usuarioIds, TipoPerfil papel)
        {
            var ids = usuarioIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<string, ResumoReputacao>();

            // Uma única consulta agrupada no SQL Server para todos os usuários da lista
            var resumos = papel == TipoPerfil.Montador
                ? await _context.PedidosMontagem
                    .Where(p => p.MontadorId != null && ids.Contains(p.MontadorId) && p.NotaMontador != null)
                    .GroupBy(p => p.MontadorId!)
                    .Select(g => new { Id = g.Key, Media = g.Average(p => (double)p.NotaMontador!.Value), Total = g.Count() })
                    .ToListAsync()
                : await _context.PedidosMontagem
                    .Where(p => ids.Contains(p.ClienteId) && p.NotaCliente != null)
                    .GroupBy(p => p.ClienteId)
                    .Select(g => new { Id = g.Key, Media = g.Average(p => (double)p.NotaCliente!.Value), Total = g.Count() })
                    .ToListAsync();

            return resumos.ToDictionary(r => r.Id, r => new ResumoReputacao(r.Media, r.Total));
        }

        public Task<bool> PossuemPedidoEmComumAsync(string usuarioA, string usuarioB) =>
            _context.PedidosMontagem.AnyAsync(p =>
                (p.ClienteId == usuarioA && p.MontadorId == usuarioB) ||
                (p.ClienteId == usuarioB && p.MontadorId == usuarioA));

        public Task<bool> PossuemServicoEmAndamentoAsync(string usuarioA, string usuarioB) =>
            _context.PedidosMontagem.AnyAsync(p =>
                p.Status == StatusPedido.EmAndamento &&
                ((p.ClienteId == usuarioA && p.MontadorId == usuarioB) ||
                 (p.ClienteId == usuarioB && p.MontadorId == usuarioA)));

        public Task<bool> ClienteTemPedidoAbertoAsync(string clienteId) =>
            _context.PedidosMontagem.AnyAsync(p => p.ClienteId == clienteId && p.Status == StatusPedido.Disponivel);

        public async Task RegistrarRecusaAsync(Guid pedidoId, string montadorId)
        {
            var jaRecusado = await _context.PedidosRecusados
                .AnyAsync(r => r.PedidoId == pedidoId && r.MontadorId == montadorId);
            if (jaRecusado)
                return;

            await _context.PedidosRecusados.AddAsync(new PedidoRecusado(pedidoId, montadorId));
            await _context.SaveChangesAsync();
        }

        public async Task<HashSet<Guid>> ObterIdsRecusadosAsync(string montadorId)
        {
            var ids = await _context.PedidosRecusados
                .Where(r => r.MontadorId == montadorId)
                .Select(r => r.PedidoId)
                .ToListAsync();
            return ids.ToHashSet();
        }

        public async Task AdicionarAsync(PedidoMontagem pedido)
        {
            await _context.PedidosMontagem.AddAsync(pedido);
            await _context.SaveChangesAsync();
            await AvisarAlteracaoAsync(pedido, null);
        }

        public async Task AtualizarAsync(PedidoMontagem pedido)
        {
            // Pedido carregado por este contexto já é rastreado: o EF grava só o que mudou.
            // Update() marcaria o grafo inteiro (cliente e montador) como alterado.
            var entrada = _context.Entry(pedido);
            if (entrada.State == EntityState.Detached)
                _context.PedidosMontagem.Update(pedido);

            // Quem era o montador antes de salvar: ao recusar/desistir ele sai do pedido,
            // mas a tela dele também precisa atualizar
            var montadorAnterior = entrada.Property(p => p.MontadorId).OriginalValue;

            await _context.SaveChangesAsync();
            await AvisarAlteracaoAsync(pedido, montadorAnterior);
        }

        // Atualiza em tempo real as telas do cliente, do(s) montador(es) e o feed da região.
        // Falha no aviso nunca desfaz a gravação (o app também recarrega ao abrir a tela).
        private async Task AvisarAlteracaoAsync(PedidoMontagem pedido, string? montadorAnterior)
        {
            try
            {
                await _tempoReal.PedidoAlteradoAsync(
                    pedido.Id,
                    Regiao.DeEndereco(pedido.EnderecoServico),
                    new[] { pedido.ClienteId, pedido.MontadorId, montadorAnterior });
            }
            catch
            {
                // Sem conexão em tempo real o app continua funcionando normalmente
            }
        }
    }
}