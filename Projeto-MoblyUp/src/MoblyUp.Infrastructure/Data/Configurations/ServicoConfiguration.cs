using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Infrastructure.Data.Configurations
{
    public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
    {
        public void Configure(EntityTypeBuilder<Servico> builder)
        {
            builder.ToTable("Servicos");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Nome)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(s => s.Descricao)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(s => s.Valor)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
        }
    }
}