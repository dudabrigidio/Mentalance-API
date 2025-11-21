
using Mentalance.Models;
using Microsoft.EntityFrameworkCore;

namespace Mentalance.Connection
{
    /// <summary>
    /// Contexto do Entity Framework para acesso ao banco de dados
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Inicializa uma nova instância do AppDbContext
        /// </summary>
        /// <param name="options">Opções de configuração do DbContext</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Conjunto de entidades Usuario no banco de dados
        /// </summary>
        public DbSet<Usuario> Usuario { get; set; }
        
        /// <summary>
        /// Conjunto de entidades Checkin no banco de dados
        /// </summary>
        public DbSet<Checkin> Checkin { get; set; }
        
        /// <summary>
        /// Conjunto de entidades AnaliseSemanal no banco de dados
        /// </summary>
        public DbSet<AnaliseSemanal> AnaliseSemanal { get; set; }
        

        /// <summary>
        /// Configura o modelo de dados durante a criação do contexto
        /// </summary>
        /// <param name="modelBuilder">Construtor do modelo</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Checkin>()
                .Property(c => c.Emocao)
                .HasConversion<string>();
        }
    }
}