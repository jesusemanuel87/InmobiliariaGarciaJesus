using Microsoft.EntityFrameworkCore;
using InmobiliariaGarciaJesus.Models;

namespace InmobiliariaGarciaJesus.Data
{
    /// <summary>
    /// DbContext principal para Entity Framework Core
    /// Maneja la conexión y mapeo con la base de datos MySQL
    /// </summary>
    public class InmobiliariaDbContext : DbContext
    {
        public InmobiliariaDbContext(DbContextOptions<InmobiliariaDbContext> options)
            : base(options)
        {
        }

        // DbSets - Representan las tablas en la base de datos
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Propietario> Propietarios { get; set; }
        public DbSet<Inquilino> Inquilinos { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Inmueble> Inmuebles { get; set; }
        public DbSet<InmuebleImagen> InmuebleImagenes { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Configuracion> Configuraciones { get; set; }
        public DbSet<TipoInmuebleEntity> TiposInmueble { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de tablas (mapeo a nombres existentes en MySQL)
            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Propietario>().ToTable("propietarios");
            modelBuilder.Entity<Inquilino>().ToTable("inquilinos");
            modelBuilder.Entity<Empleado>().ToTable("empleados");
            modelBuilder.Entity<Inmueble>().ToTable("inmuebles");
            modelBuilder.Entity<InmuebleImagen>().ToTable("inmuebleimagenes");
            modelBuilder.Entity<Contrato>().ToTable("contratos");
            modelBuilder.Entity<Pago>().ToTable("pagos");
            modelBuilder.Entity<Configuracion>().ToTable("configuracion");
            modelBuilder.Entity<TipoInmuebleEntity>().ToTable("tiposinmueble");

            // Configuración de relaciones y comportamiento de eliminación
            
            // Usuario -> Propietario/Inquilino/Empleado (opcional, uno de ellos)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Propietario)
                .WithMany()
                .HasForeignKey(u => u.PropietarioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Inquilino)
                .WithMany()
                .HasForeignKey(u => u.InquilinoId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Empleado)
                .WithMany()
                .HasForeignKey(u => u.EmpleadoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Inmueble -> Propietario
            modelBuilder.Entity<Inmueble>()
                .HasOne(i => i.Propietario)
                .WithMany()
                .HasForeignKey(i => i.PropietarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // InmuebleImagen -> Inmueble
            modelBuilder.Entity<InmuebleImagen>()
                .HasOne(img => img.Inmueble)
                .WithMany(i => i.Imagenes)
                .HasForeignKey(img => img.InmuebleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Contrato -> Inmueble
            modelBuilder.Entity<Contrato>()
                .HasOne(c => c.Inmueble)
                .WithMany(i => i.Contratos)
                .HasForeignKey(c => c.InmuebleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Contrato -> Inquilino
            modelBuilder.Entity<Contrato>()
                .HasOne(c => c.Inquilino)
                .WithMany()
                .HasForeignKey(c => c.InquilinoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Contrato -> Usuario (auditoría)
            modelBuilder.Entity<Contrato>()
                .HasOne(c => c.CreadoPor)
                .WithMany()
                .HasForeignKey(c => c.CreadoPorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Contrato>()
                .HasOne(c => c.TerminadoPor)
                .WithMany()
                .HasForeignKey(c => c.TerminadoPorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Pago -> Usuario (auditoría)
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.CreadoPor)
                .WithMany()
                .HasForeignKey(p => p.CreadoPorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.AnuladoPor)
                .WithMany()
                .HasForeignKey(p => p.AnuladoPorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuración de índices para mejorar rendimiento
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            modelBuilder.Entity<Propietario>()
                .HasIndex(p => p.Dni)
                .IsUnique();

            modelBuilder.Entity<Propietario>()
                .HasIndex(p => p.Email)
                .IsUnique();

            modelBuilder.Entity<Inmueble>()
                .HasIndex(i => i.PropietarioId);

            modelBuilder.Entity<Contrato>()
                .HasIndex(c => c.InmuebleId);

            modelBuilder.Entity<Pago>()
                .HasIndex(p => p.ContratoId);

            // Configuración de propiedades calculadas
            modelBuilder.Entity<Inmueble>()
                .Ignore(i => i.Tipo);
            
            modelBuilder.Entity<Inmueble>()
                .Ignore(i => i.ImagenPortada);
            
            modelBuilder.Entity<Inmueble>()
                .Ignore(i => i.ImagenPortadaUrl);
            
            modelBuilder.Entity<Inmueble>()
                .Ignore(i => i.GoogleMapsUrl);
            
            modelBuilder.Entity<Inmueble>()
                .Ignore(i => i.Disponible);

            // Configuración de relación Inmueble -> TipoInmuebleEntity
            modelBuilder.Entity<Inmueble>()
                .HasOne(i => i.TipoInmueble)
                .WithMany(t => t.Inmuebles)
                .HasForeignKey(i => i.TipoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de conversión de enums a enteros
            modelBuilder.Entity<Usuario>()
                .Property(u => u.Rol)
                .HasConversion<int>();

            modelBuilder.Entity<Contrato>()
                .Property(c => c.Estado)
                .HasConversion<string>();

            modelBuilder.Entity<Pago>()
                .Property(p => p.Estado)
                .HasConversion<string>();

            modelBuilder.Entity<Pago>()
                .Property(p => p.MetodoPago)
                .HasConversion<string>();

            // Estado: BD usa 1=Activo, 0=Inactivo pero enum C# usa Activo=0, Inactivo=1
            // Necesitamos invertir el mapeo
            modelBuilder.Entity<Inmueble>()
                .Property(i => i.Estado)
                .HasConversion(
                    v => v == EstadoInmueble.Activo ? 1 : 0,  // C# → BD: Activo=1, Inactivo=0
                    v => v == 1 ? EstadoInmueble.Activo : EstadoInmueble.Inactivo  // BD → C#: 1=Activo, 0=Inactivo
                );

            modelBuilder.Entity<Inmueble>()
                .Property(i => i.Uso)
                .HasConversion<string>();
        }
    }
}
