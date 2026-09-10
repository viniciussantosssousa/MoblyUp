using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Domain.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<Categoria> ObterPorIdAsync(Guid id);
        Task<IEnumerable<Categoria>> ObterTodasAtivasAsync();
        Task AdicionarAsync(Categoria categoria);
        Task AtualizarAsync(Categoria categoria);
    }
}