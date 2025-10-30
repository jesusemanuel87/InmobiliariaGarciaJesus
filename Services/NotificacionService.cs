using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;

namespace InmobiliariaGarciaJesus.Services
{
    /// <summary>
    /// Servicio para enviar notificaciones por email a propietarios
    /// </summary>
    public class NotificacionService
    {
        private readonly InmobiliariaDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificacionService> _logger;

        public NotificacionService(
            InmobiliariaDbContext context,
            IConfiguration configuration,
            ILogger<NotificacionService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Notificar al propietario cuando se registra un pago (guarda en BD para la app)
        /// </summary>
        public async Task NotificarPagoRegistrado(Pago pago, int propietarioId)
        {
            try
            {
                // Obtener datos del propietario
                var propietario = await _context.Propietarios
                    .FirstOrDefaultAsync(p => p.Id == propietarioId);

                if (propietario == null)
                {
                    _logger.LogWarning($"No se pudo enviar notificación: propietario {propietarioId} no encontrado");
                    return;
                }

                // Obtener información del contrato y inmueble
                var contrato = await _context.Contratos
                    .Include(c => c.Inmueble)
                    .Include(c => c.Inquilino)
                    .FirstOrDefaultAsync(c => c.Id == pago.ContratoId);

                if (contrato == null)
                {
                    _logger.LogWarning($"No se encontró contrato {pago.ContratoId} para notificación de pago");
                    return;
                }

                // 1. GUARDAR NOTIFICACIÓN EN LA BASE DE DATOS (para la app móvil)
                var notificacion = new Notificacion
                {
                    PropietarioId = propietarioId,
                    Tipo = TipoNotificacion.PagoRegistrado.ToString(),
                    Titulo = "💰 Pago Recibido",
                    Mensaje = $"Se ha registrado el pago de ${pago.TotalAPagar:N2} de {contrato.Inquilino?.NombreCompleto ?? "inquilino"} por el inmueble en {contrato.Inmueble?.Direccion}",
                    Datos = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        pagoId = pago.Id,
                        contratoId = pago.ContratoId,
                        inmuebleId = contrato.InmuebleId,
                        monto = pago.TotalAPagar,
                        numeroCuota = pago.Numero
                    }),
                    Leida = false,
                    FechaCreacion = DateTime.Now
                };

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Notificación guardada en BD para propietario {propietarioId}");

                // 2. OPCIONAL: Enviar email también (si tiene email configurado)
                if (!string.IsNullOrEmpty(propietario.Email))
                {
                    try
                    {
                        await EnviarEmailPagoRegistrado(pago, contrato, propietario);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "Error al enviar email, pero notificación guardada en BD");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear notificación de pago {pago.Id}");
                throw;
            }
        }

        /// <summary>
        /// Enviar email de pago registrado (opcional, además de la notificación in-app)
        /// </summary>
        private async Task EnviarEmailPagoRegistrado(Pago pago, Contrato contrato, Propietario propietario)
        {
            try
            {

                // Construir el mensaje
                var subject = $"✅ Pago Registrado - {contrato.Inmueble?.Direccion}";
                
                var body = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
        .info-row {{ margin: 10px 0; padding: 10px; background-color: white; border-left: 3px solid #28a745; }}
        .label {{ font-weight: bold; color: #555; }}
        .value {{ color: #333; }}
        .total {{ font-size: 1.2em; font-weight: bold; color: #28a745; }}
        .footer {{ text-align: center; padding: 20px; color: #777; font-size: 0.9em; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>💰 Pago de Alquiler Registrado</h2>
        </div>
        <div class='content'>
            <p>Estimado/a {propietario.NombreCompleto},</p>
            <p>Le informamos que se ha registrado un pago de alquiler:</p>
            
            <div class='info-row'>
                <span class='label'>🏠 Inmueble:</span>
                <span class='value'>{contrato.Inmueble?.Direccion}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>👤 Inquilino:</span>
                <span class='value'>{contrato.Inquilino?.NombreCompleto}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>📅 Fecha de Pago:</span>
                <span class='value'>{pago.FechaPago?.ToString("dd/MM/yyyy")}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>📋 Período:</span>
                <span class='value'>Cuota #{pago.Numero}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>💵 Importe:</span>
                <span class='value'>${pago.Importe:N2}</span>
            </div>
            
            {(pago.Intereses > 0 ? $@"
            <div class='info-row'>
                <span class='label'>📈 Intereses:</span>
                <span class='value'>${pago.Intereses:N2}</span>
            </div>" : "")}
            
            {(pago.Multas > 0 ? $@"
            <div class='info-row'>
                <span class='label'>⚠️ Multas:</span>
                <span class='value'>${pago.Multas:N2}</span>
            </div>" : "")}
            
            <div class='info-row'>
                <span class='label'>💰 Total Pagado:</span>
                <span class='value total'>${pago.TotalAPagar:N2}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>💳 Método de Pago:</span>
                <span class='value'>{pago.MetodoPago?.ToString() ?? "No especificado"}</span>
            </div>
            
            {(!string.IsNullOrEmpty(pago.Observaciones) ? $@"
            <div class='info-row'>
                <span class='label'>📝 Observaciones:</span>
                <span class='value'>{pago.Observaciones}</span>
            </div>" : "")}
            
            <p style='margin-top: 20px;'>
                Puede ver más detalles ingresando al sistema de gestión.
            </p>
        </div>
        <div class='footer'>
            <p>Este es un mensaje automático. Por favor no responda a este correo.</p>
            <p>© {DateTime.Now.Year} Inmobiliaria García Jesús</p>
        </div>
    </div>
</body>
</html>";

                await EnviarEmail(propietario.Email, subject, body);
                
                _logger.LogInformation($"Email de pago {pago.Id} enviado a {propietario.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar email de pago {pago.Id}");
                throw;
            }
        }

        /// <summary>
        /// Método privado para enviar emails usando SMTP
        /// </summary>
        private async Task EnviarEmail(string destinatario, string asunto, string cuerpoHtml)
        {
            // Configuración SMTP desde appsettings.json
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:Username"];
            var smtpPassword = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:FromEmail"] ?? smtpUsername;
            var fromName = _configuration["Email:FromName"] ?? "Inmobiliaria García Jesús";

            if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("Configuración de email no establecida. No se envió notificación.");
                return;
            }

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mailMessage.To.Add(destinatario);

            await smtpClient.SendMailAsync(mailMessage);
        }

        /// <summary>
        /// Notificar al propietario sobre un pago vencido (para uso futuro)
        /// </summary>
        public async Task NotificarPagoVencido(Pago pago, int propietarioId)
        {
            // Implementación futura para notificar pagos vencidos
            _logger.LogInformation($"Notificación de pago vencido {pago.Id} (pendiente de implementación)");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Notificar próximo vencimiento de pago (para uso futuro)
        /// </summary>
        public async Task NotificarProximoVencimiento(Pago pago, int propietarioId, int diasParaVencer)
        {
            // Implementación futura para recordatorios
            _logger.LogInformation($"Recordatorio de pago {pago.Id} (pendiente de implementación)");
            await Task.CompletedTask;
        }
    }
}
