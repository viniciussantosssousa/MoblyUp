using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Infrastructure.Data.Configurations
{
    public class MensagemChatConfiguration : IEntityTypeConfiguration<MensagemChat>
    {
        public void Configure(EntityTypeBuilder<MensagemChat> builder)
        {
            builder.ToTable("MensagensChat");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Texto)
                .IsRequired()
                .HasMaxLength(MensagemChat.TamanhoMaximo);

            builder.HasOne<PedidoMontagem>()
                .WithMany()
                .HasForeignKey(m => m.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(m => m.RemetenteId)
                .OnDelete(DeleteBehavior.Restrict);

            // A conversa é sempre lida em ordem cronológica por pedido
            builder.HasIndex(m => new { m.PedidoId, m.DataEnvio });
        }
    }
}
