using Microsoft.EntityFrameworkCore;
using InmobiliariaGarciaJesus.Models;

namespace InmobiliariaGarciaJesus.Data
{
    public class InmobiliariaContext : DbContext
    {
        public InmobiliariaContext(DbContextOptions<InmobiliariaContext> options) : base(options)
        {
        }

        public DbSet<Propietario> Propietarios { get; set; }
        public DbSet<Inquilino> Inquilinos { get; set; }
        public DbSet<Inmueble> Inmuebles { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Pago> Pagos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Propietarios
            modelBuilder.Entity<Propietario>(entity =>
            {
                entity.HasIndex(e => e.DNI).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuración de Inquilinos
            modelBuilder.Entity<Inquilino>(entity =>
            {
                entity.HasIndex(e => e.DNI).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuración de Inmuebles
            modelBuilder.Entity<Inmueble>(entity =>
            {
                entity.HasOne(d => d.Propietario)
                    .WithMany(p => p.Inmuebles)
                    .HasForeignKey(d => d.PropietarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Tipo)
                    .HasConversion<string>();

                entity.Property(e => e.Uso)
                    .HasConversion<string>();

                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuración de Contratos
            modelBuilder.Entity<Contrato>(entity =>
            {
                entity.HasOne(d => d.Inquilino)
                    .WithMany(p => p.Contratos)
                    .HasForeignKey(d => d.InquilinoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Inmueble)
                    .WithMany(p => p.Contratos)
                    .HasForeignKey(d => d.InmuebleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Estado)
                    .HasConversion<string>();

                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuración de Pagos
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.HasOne(d => d.Contrato)
                    .WithMany(p => p.Pagos)
                    .HasForeignKey(d => d.ContratoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Estado)
                    .HasConversion<string>();

                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Datos semilla
            modelBuilder.Entity<Propietario>().HasData(
                new Propietario
                {
                    Id = 1,
                    DNI = "35456987",
                    Nombre = "José",
                    Apellido = "Pérez",
                    Telefono = "2657123456",
                    Email = "jose.perez@email.com",
                    Direccion = "Av. San Martín 123, San Luis",
                    FechaCreacion = DateTime.Now,
                    Estado = true
                },
                new Propietario
                {
                    Id = 2,
                    DNI = "36987456",
                    Nombre = "María",
                    Apellido = "González",
                    Telefono = "2664896547",
                    Email = "maria.gonzalez@email.com",
                    Direccion = "Rivadavia 456, San Luis",
                    FechaCreacion = DateTime.Now,
                    Estado = true
                }
            );

            modelBuilder.Entity<Inquilino>().HasData(
                new Inquilino
                {
                    Id = 1,
                    DNI = "30111222",
                    Nombre = "Carlos",
                    Apellido = "Rodríguez",
                    Telefono = "2651111111",
                    Email = "carlos.rodriguez@email.com",
                    Direccion = "Mitre 789, San Luis",
                    FechaCreacion = DateTime.Now,
                    Estado = true
                },
                new Inquilino
                {
                    Id = 2,
                    DNI = "33000111",
                    Nombre = "Ana",
                    Apellido = "Martínez",
                    Telefono = "2652222222",
                    Email = "ana.martinez@email.com",
                    Direccion = "Belgrano 321, San Luis",
                    FechaCreacion = DateTime.Now,
                    Estado = true
                }
            );
        }
    }
}
