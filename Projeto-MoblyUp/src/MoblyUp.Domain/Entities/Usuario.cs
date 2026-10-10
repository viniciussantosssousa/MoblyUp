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

        public Usuario(string nome, string? cpf,string email, string senhaHash, string telefone, TipoPerfil perfil, string? id = null)
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

            Nome = nome.Trim();
        }

        public void DesativarConta()
        {
            Ativo = false;
        }

        public void DefinirDadosBancarios(string chavePix)
        {
            if (Perfil != TipoPerfil.Montador)
                throw new InvalidOperationException("Apenas montadores podem cadastrar chave Pix para recebimento.");
                
            ChavePix = chavePix;
        }

        public void ValidarDocumento(string urlFotoDocumento)
        {
            DocumentoFotoUrl = urlFotoDocumento;
        }
    }
}