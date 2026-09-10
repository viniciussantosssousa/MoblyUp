using System;
using System.Threading.Tasks;
using MoblyUp.Application.DTOs;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    public class CriarContaMontadorUseCase
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public CriarContaMontadorUseCase(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task ExecutarAsync(CriarContaMontadorRequest request)
        {
            var emailExistente = await _usuarioRepository.ObterPorEmailAsync(request.Email);
            if (emailExistente != null)
                throw new Exception("E-mail já cadastrado.");

            var cpfExistente = await _usuarioRepository.ObterPorCpfAsync(request.Cpf);
            if (cpfExistente != null)
                throw new Exception("CPF já cadastrado.");

            string senhaHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.Senha));

            var montador = new Usuario(
                request.Nome,
                request.Cpf,
                request.Email,
                senhaHash,
                request.Telefone,
                TipoPerfil.Montador
            );

            montador.DefinirDadosBancarios(request.ChavePix);
            montador.ValidarDocumento(request.DocumentoFotoUrl);

            await _usuarioRepository.AdicionarAsync(montador);
        }
    }
}