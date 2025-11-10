# GUÍA DE IMPLEMENTACIÓN: PRUEBAS UNITARIAS CON XUNIT.NET
## PARTE 2: CONFIGURACIÓN E INFRAESTRUCTURA

---

## 6. CONFIGURACIÓN PASO A PASO

### 6.1 FASE 1: Crear Proyecto de Tests

#### Paso 1: Crear proyecto XUnit
```bash
# Desde la carpeta raíz de la solución
cd d:\Documents\ULP\2025\NET\Proyecto\InmobiliariaGarciaJesus

# Crear proyecto de tests
dotnet new xunit -n InmobiliariaGarciaJesus.Tests

# Agregar a la solución (si existe .sln)
dotnet sln add InmobiliariaGarciaJesus.Tests/InmobiliariaGarciaJesus.Tests.csproj
```

#### Paso 2: Instalar paquetes NuGet necesarios
```bash
cd InmobiliariaGarciaJesus.Tests

# Paquetes core de XUnit
dotnet add package xunit --version 2.6.5
dotnet add package xunit.runner.visualstudio --version 2.5.6
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0

# Para testing de APIs (WebApplicationFactory)
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 9.0.0

# Para mocking (opcional, si usas mocks)
dotnet add package Moq --version 4.20.70

# Para BD en memoria
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 9.0.0

# Para aserciones fluidas (opcional pero recomendado)
dotnet add package FluentAssertions --version 6.12.0

# Para trabajar con JSON en HTTP
dotnet add package System.Net.Http.Json --version 9.0.0
```

#### Paso 3: Agregar referencia al proyecto principal
```bash
dotnet add reference ../InmobiliariaGarciaJesus/InmobiliariaGarciaJesus.csproj
```

#### Paso 4: Crear estructura de carpetas
```bash
# Desde InmobiliariaGarciaJesus.Tests/
mkdir Controllers
mkdir Helpers
mkdir Fixtures
mkdir TestData
```

---

## 7. INFRAESTRUCTURA DE TESTS

### 7.1 Archivo: `Fixtures/CustomWebApplicationFactory.cs`

**Propósito**: Configurar un servidor de pruebas con BD en memoria

**Estructura básica**:
```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using InmobiliariaGarciaJesus.Data;

namespace InmobiliariaGarciaJesus.Tests.Fixtures
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. Remover DbContext real de MySQL
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<InmobiliariaDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 2. Agregar DbContext con InMemory Database
                services.AddDbContext<InmobiliariaDbContext>(options =>
                {
                    // Cada instancia de factory usa BD única
                    options.UseInMemoryDatabase($"InmobiliariaTestDb_{Guid.NewGuid()}");
                });

                // 3. Configurar logging para tests (opcional)
                services.AddLogging(logging =>
                {
                    logging.ClearProviders(); // Evitar logs en consola
                    logging.AddDebug(); // Solo para debugging de tests
                });

                // 4. Seed inicial de datos (se ejecuta una vez al crear el factory)
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<InmobiliariaDbContext>();
                    
                    // Asegurar que la BD está creada
                    db.Database.EnsureCreated();
                    
                    // Seed de datos comunes (tipos de inmueble, etc.)
                    SeedTestData(db);
                }
            });
        }

        private void SeedTestData(InmobiliariaDbContext context)
        {
            // Datos comunes que todos los tests necesitan
            
            // 1. Tipo de Inmueble
            if (!context.TiposInmueble.Any())
            {
                context.TiposInmueble.AddRange(
                    new TipoInmuebleEntity { Id = 1, Nombre = "Casa", Estado = true },
                    new TipoInmuebleEntity { Id = 2, Nombre = "Departamento", Estado = true },
                    new TipoInmuebleEntity { Id = 3, Nombre = "Local", Estado = true }
                );
            }

            context.SaveChanges();
        }
    }
}
```

**Puntos clave**:
- ✅ Cada test obtiene una BD en memoria limpia (Guid.NewGuid())
- ✅ Seed automático de datos comunes
- ✅ Logging deshabilitado para evitar ruido en tests

---

### 7.2 Archivo: `Helpers/AuthHelper.cs`

**Propósito**: Facilitar la autenticación en tests

**Estructura básica**:
```csharp
using System.Net.Http.Json;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Models.DTOs;
using BCrypt.Net;
using InmobiliariaGarciaJesus.Data;

namespace InmobiliariaGarciaJesus.Tests.Helpers
{
    public static class AuthHelper
    {
        /// <summary>
        /// Crea un propietario de prueba con usuario y contraseña
        /// </summary>
        public static (Propietario propietario, Usuario usuario) CrearPropietarioPrueba(
            InmobiliariaDbContext context,
            string email = "jose.perez@email.com",
            string password = "123456",
            string dni = "12345678")
        {
            // Crear propietario
            var propietario = new Propietario
            {
                Nombre = "José",
                Apellido = "Pérez",
                Dni = dni,
                Email = email,
                Telefono = "2664123456",
                Direccion = "Calle Falsa 123",
                Estado = true,
                FechaCreacion = DateTime.Now
            };
            context.Propietarios.Add(propietario);
            context.SaveChanges();

            // Crear usuario asociado
            var usuario = new Usuario
            {
                Email = email,
                Username = email,
                ClaveHash = BCrypt.Net.BCrypt.HashPassword(password),
                Password = BCrypt.Net.BCrypt.HashPassword(password), // Si hay 2 campos
                Rol = RolUsuario.Propietario,
                PropietarioId = propietario.Id,
                Estado = true,
                RequiereCambioClave = false,
                FechaCreacion = DateTime.Now
            };
            context.Usuarios.Add(usuario);
            context.SaveChanges();

            return (propietario, usuario);
        }

        /// <summary>
        /// Realiza login y obtiene token JWT válido
        /// </summary>
        public static async Task<string> ObtenerTokenAsync(
            HttpClient client,
            string email = "jose.perez@email.com",
            string password = "123456")
        {
            // Preparar request de login
            var loginDto = new LoginRequestDto
            {
                Email = email,
                Password = password
            };

            // Hacer login
            var response = await client.PostAsJsonAsync("/api/AuthApi/login", loginDto);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Login falló: {response.StatusCode}");
            }

            // Extraer token de la respuesta
            var loginResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
            
            if (loginResponse?.Data?.Token == null)
            {
                throw new Exception("Token no encontrado en respuesta de login");
            }

            return loginResponse.Data.Token;
        }

        /// <summary>
        /// Crea un HttpClient con token JWT incluido en headers
        /// </summary>
        public static HttpClient CrearClienteAutenticado(
            WebApplicationFactory<Program> factory,
            string token)
        {
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            return client;
        }
    }
}
```

**Uso en tests**:
```csharp
// En el test:
var (propietario, _) = AuthHelper.CrearPropietarioPrueba(_context);
var token = await AuthHelper.ObtenerTokenAsync(_client);
var authenticatedClient = AuthHelper.CrearClienteAutenticado(_factory, token);
```

---

### 7.3 Archivo: `Helpers/TestDataBuilder.cs`

**Propósito**: Crear DTOs de prueba de forma limpia (Builder Pattern)

**Estructura básica**:
```csharp
using InmobiliariaGarciaJesus.Models.DTOs;

namespace InmobiliariaGarciaJesus.Tests.Helpers
{
    public class CrearInmuebleDtoBuilder
    {
        private readonly CrearInmuebleDto _dto;

        public CrearInmuebleDtoBuilder()
        {
            // Valores por defecto válidos
            _dto = new CrearInmuebleDto
            {
                Direccion = "Av. Test 123",
                Localidad = "San Luis",
                Provincia = "San Luis",
                TipoId = 1, // Casa
                Ambientes = 3,
                Superficie = 100m,
                Precio = 150000m,
                Uso = 0, // Residencial
                Latitud = -33.3017m,
                Longitud = -66.3378m
            };
        }

        public CrearInmuebleDtoBuilder ConDireccion(string direccion)
        {
            _dto.Direccion = direccion;
            return this;
        }

        public CrearInmuebleDtoBuilder ConPrecio(decimal precio)
        {
            _dto.Precio = precio;
            return this;
        }

        public CrearInmuebleDtoBuilder ConAmbientes(int ambientes)
        {
            _dto.Ambientes = ambientes;
            return this;
        }

        public CrearInmuebleDtoBuilder ConImagen(string base64, string nombre)
        {
            _dto.ImagenBase64 = base64;
            _dto.ImagenNombre = nombre;
            return this;
        }

        public CrearInmuebleDto Build() => _dto;
    }
}
```

**Uso en tests**:
```csharp
// Test 1: Inmueble básico sin imagen
var dto = new CrearInmuebleDtoBuilder().Build();

// Test 2: Inmueble con precio específico
var dto = new CrearInmuebleDtoBuilder()
    .ConPrecio(250000m)
    .Build();

// Test 3: Inmueble con imagen
var dto = new CrearInmuebleDtoBuilder()
    .ConImagen(ImageHelper.GenerarImagenBase64(), "test.jpg")
    .Build();
```

---

### 7.4 Archivo: `Helpers/ImageHelper.cs`

**Propósito**: Generar imágenes Base64 para tests sin archivos externos

**Estructura básica**:
```csharp
namespace InmobiliariaGarciaJesus.Tests.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Genera una imagen PNG válida de 1x1 pixel en Base64
        /// </summary>
        public static string GenerarImagenBase64()
        {
            // PNG mínimo válido (1x1 pixel rojo)
            byte[] pngBytes = new byte[]
            {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00,
                0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01,
                0x00, 0x00, 0x00, 0x01, 0x08, 0x02, 0x00, 0x00, 0x00, 0x90,
                0x77, 0x53, 0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41,
                0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00, 0x00, 0x03,
                0x01, 0x01, 0x00, 0x18, 0xDD, 0x8D, 0xB4, 0x00, 0x00, 0x00,
                0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
            };
            return Convert.ToBase64String(pngBytes);
        }

        /// <summary>
        /// Genera imagen JPEG Base64 (más grande, para tests de tamaño)
        /// </summary>
        public static string GenerarJpegBase64()
        {
            // JPEG mínimo válido
            byte[] jpegBytes = new byte[]
            {
                0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46,
                // ... (simplificado para el ejemplo)
                0xFF, 0xD9
            };
            return Convert.ToBase64String(jpegBytes);
        }
    }
}
```

---

## 8. ESTRUCTURA DE UN TEST TÍPICO

### 8.1 Patrón AAA (Arrange-Act-Assert)

```csharp
[Fact]
public async Task CrearInmueble_ConDatosValidos_RetornaCreated()
{
    // ============ ARRANGE (Preparar) ============
    // 1. Configurar WebApplicationFactory
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    // 2. Obtener contexto para seed de datos
    using var scope = factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();
    
    // 3. Crear propietario de prueba
    var (propietario, _) = AuthHelper.CrearPropietarioPrueba(context);
    
    // 4. Obtener token JWT
    var token = await AuthHelper.ObtenerTokenAsync(client);
    var authenticatedClient = AuthHelper.CrearClienteAutenticado(factory, token);
    
    // 5. Preparar DTO de prueba
    var dto = new CrearInmuebleDtoBuilder().Build();

    // ============ ACT (Actuar) ============
    // 6. Ejecutar request HTTP
    var response = await authenticatedClient.PostAsJsonAsync("/api/InmueblesApi", dto);

    // ============ ASSERT (Verificar) ============
    // 7. Verificar código de estado
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    
    // 8. Verificar respuesta
    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<InmuebleDto>>();
    Assert.NotNull(apiResponse);
    Assert.True(apiResponse.Success);
    
    var inmueble = apiResponse.Data;
    Assert.NotNull(inmueble);
    Assert.True(inmueble.Id > 0);
    Assert.Equal("Inactivo", inmueble.Estado); // Por defecto
    Assert.Equal(dto.Direccion, inmueble.Direccion);
    
    // 9. Verificar en BD
    var inmuebleDb = await context.Inmuebles.FindAsync(inmueble.Id);
    Assert.NotNull(inmuebleDb);
    Assert.Equal(propietario.Id, inmuebleDb.PropietarioId);
}
```

---

## 9. CONFIGURACIÓN ADICIONAL

### 9.1 Archivo: `appsettings.Test.json` (opcional)

Si necesitas configuraciones específicas para tests:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  },
  "Jwt": {
    "Secret": "test-secret-key-for-unit-tests-minimum-32-chars",
    "Issuer": "InmobiliariaGarciaJesus.Tests",
    "Audience": "InmobiliariaGarciaJesus.Tests",
    "ExpiryMinutes": 60
  }
}
```

### 9.2 Configuración de Visual Studio / Rider

**Test Explorer Settings**:
- ✅ Habilitar "Run tests in parallel" (XUnit lo soporta)
- ✅ Habilitar "Discover tests after build"
- ✅ Timeout por test: 30 segundos (default está bien)

---

## 10. RESUMEN DE ARCHIVOS A CREAR

**Checklist de infraestructura**:

```
✅ InmobiliariaGarciaJesus.Tests.csproj
   └─ Referencias a paquetes NuGet
   └─ Referencia al proyecto principal

✅ Fixtures/CustomWebApplicationFactory.cs
   └─ Configuración de servidor de pruebas
   └─ BD en memoria
   └─ Seed de datos comunes

✅ Helpers/AuthHelper.cs
   └─ CrearPropietarioPrueba()
   └─ ObtenerTokenAsync()
   └─ CrearClienteAutenticado()

✅ Helpers/TestDataBuilder.cs
   └─ CrearInmuebleDtoBuilder
   └─ (Otros builders según necesidad)

✅ Helpers/ImageHelper.cs
   └─ GenerarImagenBase64()
   └─ GenerarJpegBase64()

✅ appsettings.Test.json (opcional)
   └─ Configuraciones para tests
```

---

## 11. COMANDO PARA VERIFICAR SETUP

Después de configurar todo, ejecutar:

```bash
cd InmobiliariaGarciaJesus.Tests
dotnet build
dotnet test --list-tests
```

**Output esperado**:
```
The following Tests are available:
    InmobiliariaGarciaJesus.Tests.Controllers.UnitTest1.Test1
```

Si ves esto, la infraestructura está lista para comenzar a escribir tests reales.

---

**Continúa en**: GUIA_PRUEBAS_UNITARIAS_PARTE3.md (Implementación de Tests)
