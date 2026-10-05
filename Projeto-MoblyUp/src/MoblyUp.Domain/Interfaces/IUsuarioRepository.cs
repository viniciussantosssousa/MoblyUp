using System;
using System.Threading.Tasks;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorIdAsync(string id);
        Task<Usuario?> ObterPorCpfAsync(string cpf);
        Task<Usuario?> ObterPorEmailAsync(string email);
        Task AdicionarAsync(Usuario usuario);
        Task AtualizarAsync(Usuario usuario);
    }
}