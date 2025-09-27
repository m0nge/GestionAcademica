using System.Net;
using System.Net.Mail;

namespace GestionAcademica.Services
{
    public interface IEmailService
    {
        Task<bool> SendTwoFactorCodeAsync(string toEmail, string code, string userName);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendTwoFactorCodeAsync(string toEmail, string code, string userName)
        {
            try
            {
                // Obtener configuración
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var fromEmail = _configuration["Email:FromEmail"];
                var fromPassword = _configuration["Email:FromPassword"];
                var fromName = _configuration["Email:FromName"] ?? "Sistema Gestión Académica";

                // Validar configuración
                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromPassword))
                {
                    _logger.LogError("Configuración de email incompleta en appsettings.json");
                    return false;
                }

                _logger.LogInformation("Intentando enviar código 2FA a {Email} usando {SmtpHost}:{SmtpPort}", toEmail, smtpHost, smtpPort);

                // Crear el mensaje
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Código de Verificación - Sistema Gestión Académica",
                    Body = GenerateEmailBody(code, userName),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                // Configurar SMTP
                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(fromEmail, fromPassword),
                    EnableSsl = true,
                    Timeout = 20000 // 20 segundos
                };

                // Enviar email
                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Código 2FA enviado exitosamente a {Email}", toEmail);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Error SMTP enviando código 2FA a {Email}: {Message}", toEmail, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general enviando código 2FA a {Email}", toEmail);
                return false;
            }
        }

        private string GenerateEmailBody(string code, string userName)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 0; }}
                        .container {{ max-width: 600px; margin: 0 auto; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f9fa; padding: 30px; }}
                        .code-box {{ background: white; border: 2px dashed #667eea; padding: 20px; text-align: center; margin: 20px 0; border-radius: 10px; }}
                        .code {{ font-size: 32px; font-weight: bold; color: #667eea; letter-spacing: 8px; font-family: monospace; }}
                        .warning {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin-top: 20px; }}
                        .footer {{ background: #6c757d; color: white; padding: 20px; text-align: center; font-size: 12px; border-radius: 0 0 10px 10px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🎓 Sistema de Gestión Académica</h1>
                            <p>Código de Verificación de Dos Factores</p>
                        </div>
                        <div class='content'>
                            <h2>Hola, {userName}</h2>
                            <p>Has solicitado iniciar sesión en el Sistema de Gestión Académica. Para completar el proceso, utiliza el siguiente código de verificación:</p>
                            
                            <div class='code-box'>
                                <div class='code'>{code}</div>
                                <p><strong>⏰ Válido por 10 minutos</strong></p>
                            </div>
                            
                            <p>Si no has solicitado este código, ignora este mensaje y tu cuenta permanecerá segura.</p>
                            
                            <div class='warning'>
                                <strong>⚠️ Importante:</strong> Nunca compartas este código con nadie. El personal del sistema nunca te pedirá este código por teléfono o email.
                            </div>
                        </div>
                        <div class='footer'>
                            <p>Este es un mensaje automático del Sistema de Gestión Académica</p>
                            <p>Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}