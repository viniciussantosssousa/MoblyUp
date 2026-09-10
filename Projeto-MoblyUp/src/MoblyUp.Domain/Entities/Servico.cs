using System;

namespace MoblyUp.Domain.Entities
{
    public class Servico
    {
        public Guid Id { get; private set; }
        public Guid CategoriaId { get; private set; }
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public decimal Valor { get; private set; }
        public bool Ativo { get; private set; }

        public Servico(Guid categoriaId, string nome, string descricao, decimal valor)
        {
            Id = Guid.NewGuid();
            CategoriaId = categoriaId;
            Nome = nome;
            Descricao = descricao;
            
            if (valor < 0)
                throw new ArgumentException("O valor não pode ser negativo.");
                
            Valor = valor;
            Ativo = true;
        }

        public void AtualizarValor(decimal novoValor)
        {
            if (novoValor < 0)
                throw new ArgumentException("O valor não pode ser negativo.");

            Valor = novoValor;
        }

        public void Desativar()
        {
            Ativo = false;
        }
    }
}