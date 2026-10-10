using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Infrastructure.Data.Configurations
{
    public class PedidoRecusadoConfiguration : IEntityTypeConfiguration<PedidoRecusado>
    {
        public void Configure(EntityTypeBuilder<PedidoRecusado> builder)
        {
            builder.ToTable("PedidosRecusados");

            // Um montador recusa um mesmo pedido no máximo uma vez
            builder.HasKey(r => new { r.PedidoId, r.MontadorId });

            builder.HasOne<PedidoMontagem>()
                .WithMany()
                .HasForeignKey(r => r.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(r => r.MontadorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => r.MontadorId);
        }
    }
}
