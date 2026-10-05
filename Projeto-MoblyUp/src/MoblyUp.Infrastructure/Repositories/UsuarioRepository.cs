using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Infrastructure.Data;

namespace MoblyUp.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObterPorIdAsync(string id)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> ObterPorCpfAsync(string cpf)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Cpf == cpf);
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AdicionarAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}