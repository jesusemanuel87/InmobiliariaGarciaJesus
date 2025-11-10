using System.Net;
using System.Net.Http.Json;
using InmobiliariaGarciaJesus.Models.DTOs;
using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Tests.Fixtures;
using InmobiliariaGarciaJesus.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InmobiliariaGarciaJesus.Tests.Controllers
{
    /// <summary>
    /// Tests de integración para AuthApiController
    /// Valida el proceso de autenticación JWT
    /// </summary>
    public class AuthApiControllerTests : IAsyncLifetime
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;
        private InmobiliariaDbContext _context = null!;

        public async Task InitializeAsync()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();

            // Obtener contexto para seed de datos
            var scope = _factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();

            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _client.Dispose();
            await _factory.DisposeAsync();
        }

        [Fact]
        public async Task Login_ConCredencialesValidas_RetornaTokenYDatosPropietario()
        {
            // Arrange
            AuthHelper.CrearPropietarioPrueba(
                _context,
                email: "jose.perez@email.com",
                password: "123456"
            );

            var loginDto = new LoginRequestDto
            {
                Email = "jose.perez@email.com",
                Password = "123456"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/AuthApi/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);

            var loginResponse = apiResponse.Data;
            Assert.NotNull(loginResponse);
            Assert.NotNull(loginResponse.Token);
            Assert.NotEmpty(loginResponse.Token);
            Assert.NotNull(loginResponse.Propietario);
            Assert.Equal("José", loginResponse.Propietario.Nombre);
            Assert.Equal("Pérez", loginResponse.Propietario.Apellido);
            Assert.True(loginResponse.Expiracion > DateTime.UtcNow);
        }

        [Fact]
        public async Task Login_ConPasswordIncorrecta_RetornaUnauthorized()
        {
            // Arrange
            AuthHelper.CrearPropietarioPrueba(
                _context,
                email: "jose.perez@email.com",
                password: "123456"
            );

            var loginDto = new LoginRequestDto
            {
                Email = "jose.perez@email.com",
                Password = "incorrecta"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/AuthApi/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Contains("Credenciales", apiResponse.Message);
        }

        [Fact]
        public async Task Login_ConEmailInexistente_RetornaUnauthorized()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Email = "noexiste@email.com",
                Password = "123456"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/AuthApi/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
        }
    }
}
