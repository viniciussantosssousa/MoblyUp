using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Infrastructure.Data.Configurations
{
    public class PedidoMontagemConfiguration : IEntityTypeConfiguration<PedidoMontagem>
    {
        public void Configure(EntityTypeBuilder<PedidoMontagem> builder)
        {
            builder.ToTable("PedidosMontagem");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.TipoMovel)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.TamanhoMovel)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.UrlFotoMovelTemporaria)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(p => p.EnderecoServico)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(p => p.ValorProposto)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(p => p.TaxaPlataforma)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(p => p.ValorMontador)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(p => p.Status)
                .IsRequired();

            // O feed dos montadores filtra por status a cada abertura da tela
            builder.HasIndex(p => p.Status);

            builder.HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Montador)
                .WithMany()
                .HasForeignKey(p => p.MontadorId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}