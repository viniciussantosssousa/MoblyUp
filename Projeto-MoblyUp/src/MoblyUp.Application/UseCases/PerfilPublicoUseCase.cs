using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoblyUp.Application.DTOs;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;

namespace MoblyUp.Application.UseCases
{
    // Perfil que um usuário vê de outro: reputação, comentários e (quando liberado) o contato.
    // Regras de acesso:
    // - O cliente vê o montador que propôs/atende um pedido dele (e vice-versa): pedido em comum.
    // - O montador vê o cliente que tem pedido aberto no feed, para decidir se envia proposta.
    // - Telefone/WhatsApp só enquanto os dois têm um serviço pago em andamento.
    // CPF, e-mail e chave Pix nunca fazem parte do perfil público.
    public class PerfilPublicoUseCase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPedidoMontagemRepository _pedidoRepository;

        public PerfilPublicoUseCase(IUsuarioRepository usuarioRepository, IPedidoMontagemRepository pedidoRepository)
        {
            _usuarioRepository = usuarioRepository;
            _pedidoRepository = pedidoRepository;
        }

        public async Task<PerfilPublicoResponse> ObterAsync(string alvoId, string solicitanteId)
        {
            var alvo = await _usuarioRepository.ObterPorIdAsync(alvoId);
            if (alvo == null || !alvo.Ativo)
                throw new KeyNotFoundException("Usuário não encontrado.");

            if (alvoId != solicitanteId && !await PodeVerAsync(alvo.Id, alvo.Perfil, solicitanteId))
                throw new UnauthorizedAccessException("Você só pode ver o perfil de quem tem um serviço com você.");

            var avaliacoes = await _pedidoRepository.ObterAvaliacoesRecebidasAsync(alvo.Id, alvo.Perfil);
            var pedidos = alvo.Perfil == TipoPerfil.Montador
                ? await _pedidoRepository.ObterPorMontadorIdAsync(alvo.Id)
                : await _pedidoRepository.ObterPorClienteIdAsync(alvo.Id);

            var contatoLiberado = alvoId != solicitanteId
                && !string.IsNullOrEmpty(alvo.Telefone)
                && await _pedidoRepository.PossuemServicoEmAndamentoAsync(alvoId, solicitanteId);

            return new PerfilPublicoResponse
            {
                Id = alvo.Id,
                Nome = alvo.Nome,
                Tipo = alvo.Perfil == TipoPerfil.Montador ? "montador" : "cliente",
                FotoPerfilUrl = alvo.FotoPerfilUrl,
                Regiao = alvo.Regiao,
                DataCadastro = alvo.DataCadastro,
                ServicosConcluidos = pedidos.Count(p => p.Status == StatusPedido.Concluido),
                Reputacao = ReputacaoResponse.DeAvaliacoes(avaliacoes),
                ContatoLiberado = contatoLiberado,
                Telefone = contatoLiberado ? alvo.Telefone : null,
            };
        }

        private async Task<bool> PodeVerAsync(string alvoId, TipoPerfil perfilAlvo, string solicitanteId)
        {
            if (await _pedidoRepository.PossuemPedidoEmComumAsync(alvoId, solicitanteId))
                return true;

            // Montador avaliando se aceita o pedido de um cliente que está no feed
            var solicitante = await _usuarioRepository.ObterPorIdAsync(solicitanteId);
            return solicitante?.Perfil == TipoPerfil.Montador
                && perfilAlvo == TipoPerfil.Cliente
                && await _pedidoRepository.ClienteTemPedidoAbertoAsync(alvoId);
        }
    }
}
