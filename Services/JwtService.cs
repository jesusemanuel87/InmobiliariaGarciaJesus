using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using InmobiliariaGarciaJesus.Models;

namespace InmobiliariaGarciaJesus.Services
{
    /// <summary>
    /// Servicio para generación y validación de tokens JWT
    /// </summary>
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Genera un token JWT para un propietario autenticado
        /// </summary>
        public string GenerarToken(Usuario usuario, Propietario propietario)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurada");
                var issuer = jwtSettings["Issuer"] ?? "InmobiliariaAPI";
                var audience = jwtSettings["Audience"] ?? "InmobiliariaApp";
                var expiracionMinutos = int.Parse(jwtSettings["ExpirationMinutes"] ?? "1440"); // 24 horas por defecto

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
                    new Claim("PropietarioId", propietario.Id.ToString()),
                    new Claim("NombreCompleto", propietario.NombreCompleto),
                    new Claim("Dni", propietario.Dni),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                // Agregar foto de perfil si existe
                if (!string.IsNullOrEmpty(usuario.FotoPerfil))
                {
                    claims.Add(new Claim("FotoPerfil", usuario.FotoPerfil));
                }

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiracionMinutos),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation($"Token JWT generado exitosamente para usuario {usuario.Email}");
                
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar token JWT");
                throw;
            }
        }

        /// <summary>
        /// Valida un token JWT
        /// </summary>
        public ClaimsPrincipal? ValidarToken(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurada");
                var issuer = jwtSettings["Issuer"] ?? "InmobiliariaAPI";
                var audience = jwtSettings["Audience"] ?? "InmobiliariaApp";

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token JWT inválido o expirado");
                return null;
            }
        }

        /// <summary>
        /// Obtiene el PropietarioId del token
        /// </summary>
        public int? ObtenerPropietarioId(ClaimsPrincipal user)
        {
            var propietarioIdClaim = user.FindFirst("PropietarioId")?.Value;
            if (int.TryParse(propietarioIdClaim, out int propietarioId))
            {
                return propietarioId;
            }
            return null;
        }

        /// <summary>
        /// Obtiene el UsuarioId del token
        /// </summary>
        public int? ObtenerUsuarioId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
