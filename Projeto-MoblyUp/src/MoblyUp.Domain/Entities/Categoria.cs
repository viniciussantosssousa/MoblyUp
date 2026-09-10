using System;
using System.Collections.Generic;

namespace MoblyUp.Domain.Entities
{
    public class Categoria
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public bool Ativo { get; private set; }

        private readonly List<Servico> _servicos;
        public IReadOnlyCollection<Servico> Servicos => _servicos.AsReadOnly();

        public Categoria(string nome)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Ativo = true;
            _servicos = new List<Servico>();
        }

        public void AtualizarNome(string novoNome)
        {
            if (string.IsNullOrWhiteSpace(novoNome))
                throw new ArgumentException("O nome não pode ser vazio.");

            Nome = novoNome;
        }

        public void Desativar()
        {
            Ativo = false;
        }

        public void AdicionarServico(Servico servico)
        {
            if (servico == null)
                throw new ArgumentNullException(nameof(servico));

            _servicos.Add(servico);
        }
    }
}