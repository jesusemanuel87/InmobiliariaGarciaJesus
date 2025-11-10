using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;

namespace InmobiliariaGarciaJesus.Tests.Fixtures
{
    /// <summary>
    /// Factory personalizado para crear un servidor de pruebas con BD en memoria
    /// Reemplaza MySQL por InMemory para tests
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Configurar ambiente de Testing
            // Program.cs detectará automáticamente y usará InMemory en lugar de MySQL
            builder.UseEnvironment("Testing");
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            // Seed de datos comunes después de que el host esté completamente configurado
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();
                db.Database.EnsureCreated();
                SeedTestData(db);
            }

            return host;
        }

        /// <summary>
        /// Seed de datos comunes para todos los tests
        /// </summary>
        private void SeedTestData(InmobiliariaDbContext context)
        {
            // 1. Tipos de Inmueble (necesarios para crear inmuebles en tests)
            // Verificar cada uno individualmente para evitar duplicados
            if (context.TiposInmueble.Find(1) == null)
            {
                context.TiposInmueble.Add(new TipoInmuebleEntity 
                { 
                    Id = 1, 
                    Nombre = "Casa", 
                    Estado = true 
                });
            }
            
            if (context.TiposInmueble.Find(2) == null)
            {
                context.TiposInmueble.Add(new TipoInmuebleEntity 
                { 
                    Id = 2, 
                    Nombre = "Departamento", 
                    Estado = true 
                });
            }
            
            if (context.TiposInmueble.Find(3) == null)
            {
                context.TiposInmueble.Add(new TipoInmuebleEntity 
                { 
                    Id = 3, 
                    Nombre = "Local Comercial", 
                    Estado = true 
                });
            }
            
            context.SaveChanges();
        }
    }
}
