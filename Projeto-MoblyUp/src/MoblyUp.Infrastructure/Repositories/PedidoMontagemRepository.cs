using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Infrastructure.Data;

namespace MoblyUp.Infrastructure.Repositories
{
    public class PedidoMontagemRepository : IPedidoMontagemRepository
    {
        private readonly AppDbContext _context;

        public PedidoMontagemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PedidoMontagem?> ObterPorIdAsync(Guid id)
        {
            return await _context.PedidosMontagem.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PedidoMontagem>> ObterPorClienteIdAsync(string clienteId)
        {
            return await _context.PedidosMontagem
                .Where(p => p.ClienteId == clienteId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PedidoMontagem>> ObterPorMontadorIdAsync(string montadorId)
        {
            return await _context.PedidosMontagem
                .Where(p => p.MontadorId == montadorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PedidoMontagem>> ObterPorStatusAsync(StatusPedido status)
        {
            return await _context.PedidosMontagem
                .Where(p => p.Status == status)
                .ToListAsync();
        }

        public async Task AdicionarAsync(PedidoMontagem pedido)
        {
            await _context.PedidosMontagem.AddAsync(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(PedidoMontagem pedido)
        {
            _context.PedidosMontagem.Update(pedido);
            await _context.SaveChangesAsync();
        }
    }
}