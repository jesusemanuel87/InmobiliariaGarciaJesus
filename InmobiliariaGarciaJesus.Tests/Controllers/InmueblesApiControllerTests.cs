using System.Net;
using System.Net.Http.Json;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Models.DTOs;
using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Tests.Fixtures;
using InmobiliariaGarciaJesus.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InmobiliariaGarciaJesus.Tests.Controllers
{
    /// <summary>
    /// Tests de integración para InmueblesApiController
    /// Valida el flujo completo de crear inmuebles (estado Inactivo) y luego activarlos
    /// </summary>
    public class InmueblesApiControllerTests : IAsyncLifetime
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;
        private InmobiliariaDbContext _context = null!;

        public async Task InitializeAsync()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();

            // Obtener el contexto compartido desde el ServiceProvider de la factory
            var scope = _factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();

            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _client.Dispose();
            await _factory.DisposeAsync();
        }

        #region Tests de Creación de Inmueble

        [Fact]
        public async Task CrearInmueble_SinToken_RetornaUnauthorized()
        {
            // Arrange
            var dto = new CrearInmuebleDtoBuilder().Build();

            // Act
            var response = await _client.PostAsJsonAsync("/api/InmueblesApi", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CrearInmueble_ConDatosValidosSinImagen_RetornaCreatedConEstadoInactivo()
        {
            // Arrange
            var (propietario, _) = AuthHelper.CrearPropietarioPrueba(_context);
            var token = await AuthHelper.ObtenerTokenAsync(_client);
            var authenticatedClient = AuthHelper.CrearClienteAutenticado(_factory, token);

            var dto = new CrearInmuebleDtoBuilder()
                .ConDireccion("Av. Illia 456")
                .ConPrecio(180000m)
                .Build();

            // Act
            var response = await authenticatedClient.PostAsJsonAsync("/api/InmueblesApi", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<InmuebleDto>>();
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);

            var inmueble = apiResponse.Data;
            Assert.NotNull(inmueble);
            Assert.True(inmueble.Id > 0);
            Assert.Equal("Inactivo", inmueble.Estado); // ⭐ VERIFICAR ESTADO INICIAL
            Assert.Equal("Disponible", inmueble.Disponibilidad);
            Assert.Equal(dto.Direccion, inmueble.Direccion);
            Assert.Equal(dto.Precio, inmueble.Precio);
            Assert.Equal(dto.Ambientes, inmueble.Ambientes);

            // Verificar en BD
            var inmuebleDb = await _context.Inmuebles.FindAsync(inmueble.Id);
            Assert.NotNull(inmuebleDb);
            Assert.True(inmuebleDb.PropietarioId > 0); // Verificar que tiene un propietario asignado
            Assert.Equal(EstadoInmueble.Inactivo, inmuebleDb.Estado);

            // Verificar header Location
            Assert.NotNull(response.Headers.Location);
            Assert.Contains($"/api/InmueblesApi/{inmueble.Id}", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task CrearInmueble_ConImagenBase64_GuardaImagenCorrectamente()
        {
            // Arrange
            AuthHelper.CrearPropietarioPrueba(_context);
            var token = await AuthHelper.ObtenerTokenAsync(_client);
            var authenticatedClient = AuthHelper.CrearClienteAutenticado(_factory, token);

            var imagenBase64 = ImageHelper.GenerarImagenBase64();
            var imagenNombre = ImageHelper.GenerarNombreArchivo();
            var dto = new CrearInmuebleDtoBuilder()
                .ConImagen(imagenBase64, imagenNombre)
                .Build();

            // Act
            var response = await authenticatedClient.PostAsJsonAsync("/api/InmueblesApi", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<InmuebleDto>>();
            var inmueble = apiResponse!.Data;

            // Verificar que la imagen se guardó
            Assert.NotNull(inmueble!.Imagenes);
            Assert.Single(inmueble.Imagenes); // Una imagen
            Assert.True(inmueble.Imagenes[0].EsPortada); // Marcada como portada
            Assert.NotNull(inmueble.ImagenPortadaUrl); // URL no nula
            Assert.Contains("/uploads/inmuebles/", inmueble.ImagenPortadaUrl);

            // Verificar en BD
            var imagenDb = await _context.InmuebleImagenes
                .FirstOrDefaultAsync(i => i.InmuebleId == inmueble.Id);
            Assert.NotNull(imagenDb);
            Assert.True(imagenDb.EsPortada);
            Assert.NotNull(imagenDb.NombreArchivo);
        }

        [Fact]
        public async Task CrearInmueble_ConDireccionVacia_RetornaBadRequest()
        {
            // Arrange
            AuthHelper.CrearPropietarioPrueba(_context);
            var token = await AuthHelper.ObtenerTokenAsync(_client);
            var authenticatedClient = AuthHelper.CrearClienteAutenticado(_factory, token);

            var dto = new CrearInmuebleDtoBuilder()
                .ConDireccion("") // ⚠️ Inválido
                .Build();

            // Act
            var response = await authenticatedClient.PostAsJsonAsync("/api/InmueblesApi", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // Verificar que el contenido tiene un mensaje de error
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("direcci", content, StringComparison.OrdinalIgnoreCase); // Sin acento para evitar problemas de encoding
        }

        [Fact]
        public async Task CrearInmueble_ConPrecioNegativo_RetornaBadRequest()
        {
            // Arrange
            AuthHelper.CrearPropietarioPrueba(_context);
            var token = await AuthHelper.ObtenerTokenAsync(_client);
            var authenticatedClient = AuthHelper.CrearClienteAutenticado(_factory, token);

            var dto = new CrearInmuebleDtoBuilder()
                .ConPrecio(-1000m) // ⚠️ Inválido
                .Build();

            // Act
            var response = await authenticatedClient.PostAsJsonAsync("/api/InmueblesApi", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Tests de Actualización de Estado

        [Fact]
        public async Task ActualizarEstado_ActivarInmuebleInactivo_CambiaEstadoAActivo()
        {
            // Arrange: Usar email consistente para propietario y token
            var (propietario, _) = AuthHelper.CrearPropietarioPrueba(
                _context,
                email: "jose.perez@email.com",
                dni: "87654321");
            var token = await AuthHelper.ObtenerTokenAsync(_client, email: "jose.perez@email.com");
            var authenticatedClient = AuthHelper.CrearClienteAutenticado(_factory, token);

            // Crear inmueble inactivo
            var inmueble = new Inmueble
            {
                Direccion = "Test 123",
                TipoId = 1,
                Ambientes = 2,
                Precio = 100000m,
                Uso = UsoInmueble.Residencial,
                PropietarioId = propietario.Id,
                Estado = EstadoInmueble.Inactivo, // ⭐ Inactivo inicialmente
                FechaCreacion = DateTime.Now
            };
            _context.Inmuebles.Add(inmueble);
            await _context.SaveChangesAsync();

            var updateDto = new ActualizarEstadoInmuebleDtoBuilder()
                .Activo()
                .Build();

            // Act
            var response = await authenticatedClient.PatchAsync(
                $"/api/InmueblesApi/{inmueble.Id}/estado",
                JsonContent.Create(updateDto));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<InmuebleDto>>();
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);

            var inmuebleActualizado = apiResponse.Data;
            Assert.Equal("Activo", inmuebleActualizado!.Estado); // ⭐ Verificar cambio

            // Verificar en BD
            var inmuebleDb = await _context.Inmuebles.FindAsync(inmueble.Id);
            Assert.Equal(EstadoInmueble.Activo, inmuebleDb!.Estado);
        }

        [Fact]
        public async Task ActualizarEstado_DesactivarInmuebleActivo_CambiaEstadoAInactivo()
        {
            // Arrange: Usar email consistente para propietario y token
            var (propietario, _) = AuthHelper.CrearPropietarioPrueba(
                _context,
                email: "jose.perez@email.com",
                dni: "23456789");
            var token = await AuthHelper.ObtenerTokenAsync(_client, email: "jose.perez@email.com");
            var authenticatedClient = AuthHelper.CrearClienteAutenticado(_factory, token);

            // Crear inmueble ACTIVO
            var inmueble = new Inmueble
            {
                Direccion = "Test 456",
                TipoId = 1,
                Ambientes = 3,
                Precio = 150000m,
                Uso = UsoInmueble.Residencial,
                PropietarioId = propietario.Id,
                Estado = EstadoInmueble.Activo, // ⭐ Activo inicialmente
                FechaCreacion = DateTime.Now
            };
            _context.Inmuebles.Add(inmueble);
            await _context.SaveChangesAsync();

            var updateDto = new ActualizarEstadoInmuebleDtoBuilder()
                .Inactivo()
                .Build();

            // Act
            var response = await authenticatedClient.PatchAsync(
                $"/api/InmueblesApi/{inmueble.Id}/estado",
                JsonContent.Create(updateDto));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var inmuebleDb = await _context.Inmuebles.FindAsync(inmueble.Id);
            Assert.Equal(EstadoInmueble.Inactivo, inmuebleDb!.Estado);
        }

        [Fact]
        public async Task ActualizarEstado_DesactivarConContratoActivo_RetornaBadRequest()
        {
            // Arrange: Usar email consistente para propietario y token
            var (propietario, _) = AuthHelper.CrearPropietarioPrueba(
                _context,
                email: "jose.perez@email.com",
                dni: "34567890");
            var token = await AuthHelper.ObtenerTokenAsync(_client, email: "jose.perez@email.com");
            var authenticatedClient = AuthHelper.CrearClienteAutenticado(_factory, token);

            // Crear inmueble activo
            var inmueble = new Inmueble
            {
                Direccion = "Test 789",
                TipoId = 1,
                Ambientes = 2,
                Precio = 120000m,
                Uso = UsoInmueble.Residencial,
                PropietarioId = propietario.Id,
                Estado = EstadoInmueble.Activo,
                FechaCreacion = DateTime.Now
            };
            _context.Inmuebles.Add(inmueble);
            await _context.SaveChangesAsync();

            // Crear inquilino
            var inquilino = new Inquilino
            {
                Nombre = "Juan",
                Apellido = "Test",
                Dni = "99887766",
                Email = "juan@test.com",
                Estado = true,
                FechaCreacion = DateTime.Now
            };
            _context.Inquilinos.Add(inquilino);
            await _context.SaveChangesAsync();

            // Crear contrato ACTIVO asociado
            var contrato = new Contrato
            {
                InmuebleId = inmueble.Id,
                InquilinoId = inquilino.Id,
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddYears(1),
                Precio = 120000m,
                Estado = EstadoContrato.Activo, // ⭐ Contrato ACTIVO
                FechaCreacion = DateTime.Now
            };
            _context.Contratos.Add(contrato);
            await _context.SaveChangesAsync();

            var updateDto = new ActualizarEstadoInmuebleDtoBuilder()
                .Inactivo()
                .Build();

            // Act
            var response = await authenticatedClient.PatchAsync(
                $"/api/InmueblesApi/{inmueble.Id}/estado",
                JsonContent.Create(updateDto));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Contains("contrato", apiResponse.Message, StringComparison.OrdinalIgnoreCase);

            // Verificar que el estado NO cambió
            var inmuebleDb = await _context.Inmuebles.FindAsync(inmueble.Id);
            Assert.Equal(EstadoInmueble.Activo, inmuebleDb!.Estado); // Sigue activo
        }

        [Fact]
        public async Task ActualizarEstado_InmuebleDeOtroPropietario_RetornaForbidden()
        {
            // Arrange
            // Crear propietario A (autenticado)
            var (propietarioA, _) = AuthHelper.CrearPropietarioPrueba(
                _context,
                email: "propietarioA@email.com",
                dni: "11111111"
            );

            // Crear propietario B (dueño del inmueble)
            var propietarioB = new Propietario
            {
                Nombre = "Propietario",
                Apellido = "B",
                Dni = "22222222",
                Email = "propietarioB@email.com",
                Estado = true,
                FechaCreacion = DateTime.Now
            };
            _context.Propietarios.Add(propietarioB);
            await _context.SaveChangesAsync();

            // Crear inmueble del propietario B
            var inmueble = new Inmueble
            {
                Direccion = "Inmueble de B",
                TipoId = 1,
                Ambientes = 2,
                Precio = 100000m,
                Uso = UsoInmueble.Residencial,
                PropietarioId = propietarioB.Id, // ⭐ Pertenece a B
                Estado = EstadoInmueble.Inactivo,
                FechaCreacion = DateTime.Now
            };
            _context.Inmuebles.Add(inmueble);
            await _context.SaveChangesAsync();

            // Autenticar como propietario A
            var token = await AuthHelper.ObtenerTokenAsync(_client, "propietarioA@email.com");
            var authenticatedClient = AuthHelper.CrearClienteAutenticado(_factory, token);

            var updateDto = new ActualizarEstadoInmuebleDtoBuilder()
                .Activo()
                .Build();

            // Act
            var response = await authenticatedClient.PatchAsync(
                $"/api/InmueblesApi/{inmueble.Id}/estado",
                JsonContent.Create(updateDto));

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            Assert.Contains("permiso", apiResponse!.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}
