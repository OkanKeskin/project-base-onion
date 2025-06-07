using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Business.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly SmtpClient _smtpClient;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // Get SMTP settings from configuration
        var server = _configuration["SmtpSettings:Server"] ?? "smtp.example.com";
        var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
        var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"] ?? "true");
        var username = _configuration["SmtpSettings:Username"] ?? "username";
        var password = _configuration["SmtpSettings:Password"] ?? "password";
        var useDefaultCredentials = bool.Parse(_configuration["SmtpSettings:UseDefaultCredentials"] ?? "false");
        _senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? "noreply@example.com";
        _senderName = _configuration["SmtpSettings:SenderName"] ?? "Flowia";
        
        // Configure SMTP client
        _smtpClient = new SmtpClient(server, port)
        {
            EnableSsl = enableSsl,
            UseDefaultCredentials = useDefaultCredentials
        };

        if (!useDefaultCredentials)
        {
            _smtpClient.Credentials = new NetworkCredential(username, password);
        }
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetLink)
    {
        var subject = "Şifre Sıfırlama İsteği";
        var body = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <title>Şifre Sıfırlama</title>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #4A90E2; padding: 20px; text-align: center; }}
                .header h1 {{ color: white; margin: 0; }}
                .content {{ padding: 20px; }}
                .footer {{ background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #888; }}
                .button {{ display: inline-block; background-color: #4A90E2; color: white; text-decoration: none; padding: 10px 20px; border-radius: 4px; margin: 20px 0; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>Şifre Sıfırlama</h1>
                </div>
                <div class='content'>
                    <p>Merhaba,</p>
                    <p>Flowia hesabınız için şifre sıfırlama talebinde bulundunuz. Şifrenizi sıfırlamak için aşağıdaki butona tıklayın:</p>
                    <p style='text-align: center;'>
                        <a href='{resetLink}' class='button'>Şifremi Sıfırla</a>
                    </p>
                    <p>Ya da bu bağlantıyı tarayıcınıza kopyalayın:</p>
                    <p style='word-break: break-all;'>{resetLink}</p>
                    <p>Eğer bu isteği siz yapmadıysanız, lütfen bu e-postayı dikkate almayın ve güvenliğiniz için hesabınızın şifresini değiştirmeyi düşünün.</p>
                    <p>Bu bağlantı 1 saat sonra geçerliliğini yitirecektir.</p>
                </div>
                <div class='footer'>
                    <p>&copy; {DateTime.Now.Year} Flowia. Tüm hakları saklıdır.</p>
                </div>
            </div>
        </body>
        </html>
        ";
        
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendEmailVerificationAsync(string email, string verificationLink)
    {
        var subject = "E-posta Doğrulama";
        var body = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <title>E-posta Doğrulama</title>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #4A90E2; padding: 20px; text-align: center; }}
                .header h1 {{ color: white; margin: 0; }}
                .content {{ padding: 20px; }}
                .footer {{ background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #888; }}
                .button {{ display: inline-block; background-color: #4A90E2; color: white; text-decoration: none; padding: 10px 20px; border-radius: 4px; margin: 20px 0; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>E-posta Doğrulama</h1>
                </div>
                <div class='content'>
                    <p>Merhaba,</p>
                    <p>Flowia'ya kayıt olduğunuz için teşekkür ederiz. Hesabınızı aktifleştirmek için lütfen e-posta adresinizi doğrulayın:</p>
                    <p style='text-align: center;'>
                        <a href='{verificationLink}' class='button'>E-postamı Doğrula</a>
                    </p>
                    <p>Ya da bu bağlantıyı tarayıcınıza kopyalayın:</p>
                    <p style='word-break: break-all;'>{verificationLink}</p>
                    <p>Eğer Flowia'ya kayıt olmadıysanız, lütfen bu e-postayı dikkate almayın.</p>
                </div>
                <div class='footer'>
                    <p>&copy; {DateTime.Now.Year} Flowia. Tüm hakları saklıdır.</p>
                </div>
            </div>
        </body>
        </html>
        ";
        
        await SendEmailAsync(email, subject, body);
    }
    
    private async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            
            mailMessage.To.Add(to);
            
            // Actually send the email
            await _smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"E-posta gönderme hatası: {ex.Message}");
            // Re-throw to be handled by the caller
            throw;
        }
    }
} 