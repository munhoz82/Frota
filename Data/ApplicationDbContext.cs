using Microsoft.EntityFrameworkCore;
using FrotaTaxi.Models;

namespace FrotaTaxi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<CentroCusto> CentrosCusto { get; set; }
        public DbSet<UsuarioAutorizado> UsuariosAutorizados { get; set; }
        public DbSet<Unidade> Unidades { get; set; }
        public DbSet<Trecho> Trechos { get; set; }
        public DbSet<Corrida> Corridas { get; set; }
        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<Funcionalidade> Funcionalidades { get; set; }
        public DbSet<PerfilFuncionalidade> PerfilFuncionalidades { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<CentroCusto>()
                .HasOne(cc => cc.Cliente)
                .WithMany(c => c.CentrosCusto)
                .HasForeignKey(cc => cc.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UsuarioAutorizado>()
                .HasOne(ua => ua.Cliente)
                .WithMany(c => c.UsuariosAutorizados)
                .HasForeignKey(ua => ua.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trecho>()
                .HasOne(t => t.Cliente)
                .WithMany(c => c.Trechos)
                .HasForeignKey(t => t.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Corrida>()
                .HasOne(c => c.Cliente)
                .WithMany(cl => cl.Corridas)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Corrida>()
                .HasOne(c => c.Solicitante)
                .WithMany(ua => ua.Corridas)
                .HasForeignKey(c => c.SolicitanteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Corrida>()
                .HasOne(c => c.Trecho)
                .WithMany(t => t.Corridas)
                .HasForeignKey(c => c.TrechoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Corrida>()
                .HasOne(c => c.Unidade)
                .WithMany(u => u.Corridas)
                .HasForeignKey(c => c.UnidadeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Corrida>()
                .HasOne(c => c.CentroCusto)
                .WithMany(cc => cc.Corridas)
                .HasForeignKey(c => c.CentroCustoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PerfilFuncionalidade>()
                .HasOne(pf => pf.Perfil)
                .WithMany(p => p.PerfilFuncionalidades)
                .HasForeignKey(pf => pf.PerfilId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PerfilFuncionalidade>()
                .HasOne(pf => pf.Funcionalidade)
                .WithMany(f => f.PerfilFuncionalidades)
                .HasForeignKey(pf => pf.FuncionalidadeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Perfil)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(u => u.PerfilId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Funcionalidades
            modelBuilder.Entity<Funcionalidade>().HasData(
                new Funcionalidade { Id = 1, Nome = "Clientes", Controller = "Clientes", Action = "Index" },
                new Funcionalidade { Id = 2, Nome = "Unidades", Controller = "Unidades", Action = "Index" },
                new Funcionalidade { Id = 3, Nome = "Trechos", Controller = "Trechos", Action = "Index" },
                new Funcionalidade { Id = 4, Nome = "Corridas", Controller = "Corridas", Action = "Index" },
                new Funcionalidade { Id = 5, Nome = "Relatórios", Controller = "Relatorios", Action = "Index" },
                new Funcionalidade { Id = 6, Nome = "Perfis", Controller = "Perfis", Action = "Index" },
                new Funcionalidade { Id = 7, Nome = "Usuários", Controller = "Usuarios", Action = "Index" }
            );

            // Seed Perfil Admin
            modelBuilder.Entity<Perfil>().HasData(
                new Perfil { Id = 1, Nome = "Administrador" }
            );

            // Seed PerfilFuncionalidade for Admin (full access)
            for (int i = 1; i <= 7; i++)
            {
                modelBuilder.Entity<PerfilFuncionalidade>().HasData(
                    new PerfilFuncionalidade 
                    { 
                        Id = i, 
                        PerfilId = 1, 
                        FuncionalidadeId = i, 
                        PodeConsultar = true, 
                        PodeEditar = true 
                    }
                );
            }

            // Seed Admin User
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario 
                { 
                    Id = 1, 
                    Nome = "Administrador", 
                    Login = "admin", 
                    Email = "admin@frotataxi.com", 
                    Senha = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918", // "admin" hashed
                    PerfilId = 1,
                    Ativo = true,
                    DataCriacao = new DateTime(2025, 1, 1)
                }
            );
        }
    }
}
