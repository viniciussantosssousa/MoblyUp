using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Infrastructure.Data.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Nome)
                .IsRequired()
                .HasMaxLength(150);

            // CPF opcional: usuários vindos do Firebase ainda não informam CPF.
            // O índice único ignora valores nulos.
            builder.Property(u => u.Cpf)
                .IsRequired(false)
                .HasMaxLength(11);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(u => u.Cpf).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(u => u.SenhaHash)
                .IsRequired();

            builder.Property(u => u.Telefone)
                .HasMaxLength(20);

            builder.Property(u => u.ChavePix)
                .HasMaxLength(100);
        }
    }
}