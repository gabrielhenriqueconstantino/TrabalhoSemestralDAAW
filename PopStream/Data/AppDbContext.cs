using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PopStream.Models;

namespace PopStream.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Producao> Producoes { get; set; }
        public DbSet<Artista> Artistas { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<ProducaoGenero> ProducaoGeneros { get; set; }
        public DbSet<ProducaoArtista> ProducaoArtistas { get; set; }
        public DbSet<Contato> Contatos { get; set; }

        // tabela usada pelo Painel Admin
        public DbSet<UsuarioAdmin> UsuariosAdmin { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // MUITO IMPORTANTE: Identity precisa disso
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProducaoGenero>()
                .HasKey(pg => new { pg.ProducaoId, pg.GeneroId });

            modelBuilder.Entity<ProducaoArtista>()
                .HasKey(pa => new { pa.ProducaoId, pa.ArtistaId });

            modelBuilder.Entity<ProducaoGenero>()
                .HasOne(pg => pg.Producao)
                .WithMany(p => p.Generos)
                .HasForeignKey(pg => pg.ProducaoId);

            modelBuilder.Entity<ProducaoGenero>()
                .HasOne(pg => pg.Genero)
                .WithMany()
                .HasForeignKey(pg => pg.GeneroId);

            modelBuilder.Entity<ProducaoArtista>()
                .HasOne(pa => pa.Producao)
                .WithMany(p => p.Artistas)
                .HasForeignKey(pa => pa.ProducaoId);

            modelBuilder.Entity<ProducaoArtista>()
                .HasOne(pa => pa.Artista)
                .WithMany(a => a.Producoes)
                .HasForeignKey(pa => pa.ArtistaId);
        }
    }
}
