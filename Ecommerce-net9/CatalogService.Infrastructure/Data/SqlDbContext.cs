using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure
{
    public class SqlDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; } // Tabela de produtos

        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração inicial do mapeamento da entidade Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id); // Define o Id como chave primária
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100); // Nome é obrigatório e tem limite de 100 caracteres
                entity.Property(p => p.Price).IsRequired(); // Preço é obrigatório
                entity.Property(p => p.Stock).IsRequired(); // Estoque é obrigatório
            });
        }
    }
}