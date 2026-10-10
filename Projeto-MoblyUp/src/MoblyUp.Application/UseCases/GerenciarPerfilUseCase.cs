using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoblyUp.Application.DTOs;
using MoblyUp.Domain.Entities;
using MoblyUp.Domain.Enums;
using MoblyUp.Domain.Interfaces;
using MoblyUp.Domain.ValueObjects;

namespace MoblyUp.Application.UseCases
{
    // Perfil do usuário logado (cliente ou montador). O ID sempre vem do token do Firebase.
    public class GerenciarPerfilUseCase
    {
        private static readonly StatusPedido[] StatusAtivos =
        {
            StatusPedido.AguardandoAprovacaoCliente,
            StatusPedido.AguardandoPagamento,
            StatusPedido.EmAndamento,
        };

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPedidoMontagemRepository _pedidoRepository;

        public GerenciarPerfilUseCase(IUsuarioRepository usuarioRepository, IPedidoMontagemRepository pedidoRepository)
        {
            _usuarioRepository = usuarioRepository;
            _pedidoRepository = pedidoRepository;
        }

        // Cria o usuário na API com o mesmo UID do Firebase (idempotente)
        public async Task<bool> SincronizarAsync(string id, string? emailDoToken, SincronizarUsuarioRequest request)
        {
            var existente = await _usuarioRepository.ObterPorIdAsync(id);
            if (existente != null)
            {
                var alterou = false;

                // Contas antigas ainda não tinham a região salva na API
                if (!string.IsNullOrWhiteSpace(request.Regiao) && existente.Regiao != request.Regiao.Trim())
                {
                    existente.DefinirRegiao(request.Regiao);
                    alterou = true;
                }

                // O app pode sincronizar a conta logo após o login no Firebase, antes do cadastro
                // enviar o CPF: nesse caso o CPF chega numa segunda chamada e precisa ser gravado
                if (!existente.PossuiCpf && !string.IsNullOrWhiteSpace(request.Cpf))
                {
                    await DefinirCpfAsync(existente, request.Cpf);
                    alterou = true;
                }

                if (alterou)
                    await _usuarioRepository.AtualizarAsync(existente);
                return false;
            }

            var email = emailDoToken ?? request.Email;
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("E-mail não informado.");

            var perfil = request.Tipo.ToLower() == "montador" ? TipoPerfil.Montador : TipoPerfil.Cliente;

            var usuario = new Usuario(
                request.Nome,
                null,
                email,
                "", // Senha vazia pois o Firebase já cuida da segurança
                "",
                perfil,
                id
            );
            usuario.DefinirRegiao(request.Regiao);

            // O cadastro do app sempre envia o CPF. Contas antigas (sem CPF) são criadas
            // mesmo assim e o app exige que completem o cadastro antes de usar.
            if (!string.IsNullOrWhiteSpace(request.Cpf))
                await DefinirCpfAsync(usuario, request.Cpf);

            await _usuarioRepository.AdicionarAsync(usuario);
            return true;
        }

        // Completar cadastro: contas criadas antes do CPF ser obrigatório
        public async Task CadastrarCpfAsync(string id, string cpf)
        {
            var usuario = await ObterAtivoAsync(id);
            await DefinirCpfAsync(usuario, cpf);
            await _usuarioRepository.AtualizarAsync(usuario);
        }

        private async Task DefinirCpfAsync(Usuario usuario, string cpf)
        {
            var numeros = Cpf.SomenteNumeros(cpf);
            var dono = await _usuarioRepository.ObterPorCpfAsync(numeros);
            if (dono != null && dono.Id != usuario.Id)
                throw new InvalidOperationException("Este CPF já está cadastrado em outra conta.");

            usuario.DefinirCpf(numeros);
        }

        public async Task<PerfilResponse?> ObterAsync(string id)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(id);
            if (usuario == null || !usuario.Ativo)
                return null;

            var pedidos = await ObterPedidosAsync(usuario);
            var montador = usuario.Perfil == TipoPerfil.Montador;
            var concluidos = pedidos.Where(p => p.Status == StatusPedido.Concluido).ToList();

            return new PerfilResponse
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Tipo = montador ? "montador" : "cliente",
                CpfCadastrado = usuario.PossuiCpf,
                CpfMascarado = Cpf.Mascarar(usuario.Cpf),
                Telefone = string.IsNullOrEmpty(usuario.Telefone) ? null : usuario.Telefone,
                Regiao = usuario.Regiao,
                ChavePix = usuario.ChavePix,
                FotoPerfilUrl = usuario.FotoPerfilUrl,
                DataCadastro = usuario.DataCadastro,
                Estatisticas = new EstatisticasPerfil
                {
                    Total = pedidos.Count(p => p.Status != StatusPedido.Cancelado),
                    EmAndamento = pedidos.Count(p => StatusAtivos.Contains(p.Status)),
                    Concluidos = concluidos.Count,
                    ValorTotal = concluidos.Sum(p => (montador ? p.ValorMontador : p.ValorProposto) ?? 0),
                },
            };
        }

        public async Task AtualizarAsync(string id, AtualizarPerfilRequest request)
        {
            var usuario = await ObterAtivoAsync(id);

            usuario.AtualizarNome(request.Nome);

            if (request.Telefone != null)
                usuario.AtualizarTelefone(request.Telefone);

            if (request.Regiao != null)
                usuario.DefinirRegiao(request.Regiao);

            if (request.FotoPerfilUrl != null)
                usuario.DefinirFotoPerfil(request.FotoPerfilUrl);

            // A chave Pix é usada para o repasse do valor líquido ao montador
            if (usuario.Perfil == TipoPerfil.Montador && request.ChavePix != null)
                usuario.DefinirDadosBancarios(request.ChavePix);

            await _usuarioRepository.AtualizarAsync(usuario);
        }

        public async Task RegistrarPushTokenAsync(string id, string token)
        {
            var usuario = await ObterAtivoAsync(id);

            // Se outra conta entrou antes neste mesmo aparelho, ela deixa de receber aqui
            foreach (var outro in await _usuarioRepository.ObterPorPushTokenAsync(token))
            {
                if (outro.Id == usuario.Id) continue;
                outro.RemoverPushToken();
                await _usuarioRepository.AtualizarAsync(outro);
            }

            usuario.RegistrarPushToken(token);
            await _usuarioRepository.AtualizarAsync(usuario);
        }

        public async Task RemoverPushTokenAsync(string id)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(id);
            if (usuario == null) return;

            usuario.RemoverPushToken();
            await _usuarioRepository.AtualizarAsync(usuario);
        }

        // Exclui a conta na API. Bloqueia se houver serviço em negociação ou em andamento,
        // para nenhum cliente ou montador ficar "no meio" de um pedido sem a outra parte.
        public async Task ExcluirContaAsync(string id)
        {
            var usuario = await ObterAtivoAsync(id);
            var pedidos = await ObterPedidosAsync(usuario);

            if (pedidos.Any(p => StatusAtivos.Contains(p.Status)))
                throw new InvalidOperationException(
                    "Você tem serviços em negociação ou em andamento. Conclua ou cancele esses pedidos antes de excluir a conta.");

            // Pedidos do cliente ainda abertos (sem montador) são cancelados junto com a conta
            if (usuario.Perfil == TipoPerfil.Cliente)
            {
                foreach (var aberto in pedidos.Where(p => p.Status == StatusPedido.Disponivel))
                {
                    var pedido = await _pedidoRepository.ObterPorIdAsync(aberto.Id);
                    if (pedido == null) continue;
                    pedido.Cancelar();
                    await _pedidoRepository.AtualizarAsync(pedido);
                }
            }

            usuario.DesativarConta();
            await _usuarioRepository.AtualizarAsync(usuario);
        }

        private async Task<Usuario> ObterAtivoAsync(string id)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(id);
            if (usuario == null || !usuario.Ativo)
                throw new KeyNotFoundException("Usuário não encontrado.");
            return usuario;
        }

        private async Task<List<PedidoMontagem>> ObterPedidosAsync(Usuario usuario)
        {
            var pedidos = usuario.Perfil == TipoPerfil.Montador
                ? await _pedidoRepository.ObterPorMontadorIdAsync(usuario.Id)
                : await _pedidoRepository.ObterPorClienteIdAsync(usuario.Id);
            return pedidos.ToList();
        }
    }
}
