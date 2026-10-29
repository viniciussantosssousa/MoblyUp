using Microsoft.EntityFrameworkCore;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Infrastructure.Data;

namespace MoblyUp.Infrastructure.Repositories
{
    public class MensagemChatRepository : IMensagemChatRepository
    {
        private readonly AppDbContext _context;

        public MensagemChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(MensagemChat mensagem)
        {
            await _context.MensagensChat.AddAsync(mensagem);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<MensagemChat>> ObterDoPedidoAsync(Guid pedidoId) =>
            await _context.MensagensChat
                .AsNoTracking()
                .Where(m => m.PedidoId == pedidoId)
                .OrderBy(m => m.DataEnvio)
                .ToListAsync();

        public async Task<IReadOnlyList<string>> ObterTextosRecentesAsync(Guid pedidoId, string remetenteId, TimeSpan janela, int limite)
        {
            var desde = DateTime.UtcNow - janela;
            return await _context.MensagensChat
                .AsNoTracking()
                .Where(m => m.PedidoId == pedidoId && m.RemetenteId == remetenteId && m.DataEnvio >= desde)
                .OrderByDescending(m => m.DataEnvio)
                .Take(limite)
                .Select(m => m.Texto)
                .ToListAsync();
        }

        public async Task<int> MarcarComoLidasAsync(Guid pedidoId, string leitorId)
        {
            // Atualização direta no banco, sem carregar as mensagens
            return await _context.MensagensChat
                .Where(m => m.PedidoId == pedidoId && m.RemetenteId != leitorId && m.DataLeitura == null)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.DataLeitura, DateTime.UtcNow));
        }

        public async Task<Dictionary<Guid, int>> ContarNaoLidasAsync(string usuarioId, IEnumerable<Guid> pedidoIds)
        {
            var ids = pedidoIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<Guid, int>();

            return await _context.MensagensChat
                .Where(m => ids.Contains(m.PedidoId) && m.RemetenteId != usuarioId && m.DataLeitura == null)
                .GroupBy(m => m.PedidoId)
                .Select(g => new { PedidoId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(g => g.PedidoId, g => g.Total);
        }
    }
}
