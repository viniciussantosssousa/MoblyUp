using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Domain.Interfaces
{
    public interface IServicoRepository
    {
        Task<Servico> ObterPorIdAsync(Guid id);
        Task<IEnumerable<Servico>> ObterPorCategoriaAsync(Guid categoriaId);
        Task AdicionarAsync(Servico servico);
        Task AtualizarAsync(Servico servico);
    }
}