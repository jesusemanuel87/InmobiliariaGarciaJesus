using System.Net.Http.Json;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Models.DTOs;
using InmobiliariaGarciaJesus.Data;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InmobiliariaGarciaJesus.Tests.Helpers
{
    /// <summary>
    /// Helper para facilitar la autenticación en tests
    /// </summary>
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
            // 1. Crear propietario
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

            // 2. Crear usuario asociado con password hasheado
            var usuario = new Usuario
            {
                NombreUsuario = email,
                Email = email,
                ClaveHash = BCrypt.Net.BCrypt.HashPassword(password),
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
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Login falló con status {response.StatusCode}: {error}");
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

        /// <summary>
        /// Crea propietario + obtiene token + retorna cliente autenticado (todo en uno)
        /// </summary>
        public static async Task<(HttpClient client, Propietario propietario, string token)> CrearClienteAutenticadoCompleto(
            WebApplicationFactory<Program> factory,
            InmobiliariaDbContext context,
            string email = "jose.perez@email.com",
            string password = "123456")
        {
            // 1. Crear propietario
            var (propietario, _) = CrearPropietarioPrueba(context, email, password);
            
            // 2. Obtener token
            var clientTemp = factory.CreateClient();
            var token = await ObtenerTokenAsync(clientTemp, email, password);
            
            // 3. Crear cliente autenticado
            var client = CrearClienteAutenticado(factory, token);
            
            return (client, propietario, token);
        }
    }
}
