using Microsoft.EntityFrameworkCore;
using MoblyUp.Domain.Entities;

namespace MoblyUp.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<PedidoMontagem> PedidosMontagem { get; set; }
        public DbSet<PedidoRecusado> PedidosRecusados { get; set; }
        public DbSet<MensagemChat> MensagensChat { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        // Todas as datas são gravadas em UTC (DateTime.UtcNow), mas o SQL Server devolve sem o
        // "fuso". Marcar como UTC faz a API enviar "...Z" e o app converter para o horário local
        // (sem isso, uma mensagem das 22:02 aparecia como 01:02).
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTime>().HaveConversion<DataUtcConverter>();
            configurationBuilder.Properties<DateTime?>().HaveConversion<DataUtcNulavelConverter>();
        }

        private class DataUtcConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>
        {
            public DataUtcConverter() : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) { }
        }

        private class DataUtcNulavelConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>
        {
            public DataUtcNulavelConverter()
                : base(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v) { }
        }
    }
}