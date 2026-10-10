using System;
using MoblyUp.Domain.Enums;

namespace MoblyUp.Domain.Entities
{
    public class Usuario
    {
        public string Id { get; private set; }
        public string Nome { get; private set; }
        public string? Cpf { get; private set; }
        public string Email { get; private set; }
        public string SenhaHash { get; private set; }
        public string Telefone { get; private set; }
        public TipoPerfil Perfil { get; private set; }
        public DateTime DataCadastro { get; private set; }
        public bool Ativo { get; private set; }

        public string? FotoPerfilUrl { get; private set; }
        public string? ChavePix { get; private set; } 
        public string? DocumentoFotoUrl { get; private set; }

        // Região no formato "Cidade - UF" (vem do CEP no cadastro), usada para avisar montadores próximos
        public string? Regiao { get; private set; }
        // Token do Expo Push do aparelho em que o usuário está logado
        public string? ExpoPushToken { get; private set; }

        public Usuario(string nome, string? cpf, string email,string senhaHash, string telefone, TipoPerfil perfil, string? id = null)
        {
            Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id; 
            Nome = nome;
            Cpf = cpf;
            Email = email;
            SenhaHash = senhaHash;
            Telefone = telefone;
            Perfil = perfil;
            DataCadastro = DateTime.UtcNow; 
            Ativo = true;
        }

        public void AtualizarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new InvalidOperationException("O nome não pode ficar vazio.");

            if (nome.Trim().Length > 150)
                throw new InvalidOperationException("O nome pode ter no máximo 150 caracteres.");

            Nome = nome.Trim();
        }

        public void AtualizarTelefone(string? telefone)
        {
            var somenteNumeros = new string((telefone ?? "").Where(char.IsDigit).ToArray());
            if (somenteNumeros.Length != 0 && (somenteNumeros.Length < 10 || somenteNumeros.Length > 11))
                throw new InvalidOperationException("Telefone inválido. Informe DDD + número.");

            Telefone = somenteNumeros;
        }

        public void DefinirFotoPerfil(string? url)
        {
            FotoPerfilUrl = string.IsNullOrWhiteSpace(url) ? null : url;
        }

        public void DefinirRegiao(string? regiao)
        {
            Regiao = string.IsNullOrWhiteSpace(regiao) ? null : regiao.Trim();
        }

        public void RegistrarPushToken(string token)
        {
            if (!token.StartsWith("ExponentPushToken[") && !token.StartsWith("ExpoPushToken["))
                throw new InvalidOperationException("Token de notificação inválido.");

            ExpoPushToken = token;
        }

        public void RemoverPushToken()
        {
            ExpoPushToken = null;
        }

        // Exclusão de conta: mantém o histórico dos pedidos, mas apaga os dados pessoais
        // e libera o e-mail para um novo cadastro (o índice de e-mail é único).
        public void DesativarConta()
        {
            Ativo = false;
            Nome = "Conta excluída";
            Email = $"excluido-{Id}@moblyup.invalid";
            Cpf = null;
            Telefone = "";
            ChavePix = null;
            FotoPerfilUrl = null;
            DocumentoFotoUrl = null;
            ExpoPushToken = null;
        }

        public void DefinirDadosBancarios(string chavePix)
        {
            if (Perfil != TipoPerfil.Montador)
                throw new InvalidOperationException("Apenas montadores podem cadastrar chave Pix para recebimento.");

            if (string.IsNullOrWhiteSpace(chavePix))
                throw new InvalidOperationException("A chave Pix não pode ficar vazia.");

            if (chavePix.Trim().Length > 100)
                throw new InvalidOperationException("A chave Pix é muito longa.");

            ChavePix = chavePix.Trim();
        }

        public void ValidarDocumento(string urlFotoDocumento)
        {
            DocumentoFotoUrl = urlFotoDocumento;
        }
    }
}