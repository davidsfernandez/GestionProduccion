/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited. 
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Services.Interfaces;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace GestionProduccion.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var host = _configuration["SMTP_HOST"] ?? _configuration["Smtp:Host"] ?? "localhost";
            var portStr = _configuration["SMTP_PORT"] ?? _configuration["Smtp:Port"] ?? "587";
            var port = int.Parse(portStr);
            var username = _configuration["SMTP_USER"] ?? _configuration["Smtp:Username"];
            var password = _configuration["SMTP_PASS"] ?? _configuration["Smtp:Password"];
            
            var fromEmail = _configuration["SMTP_FROM_EMAIL"] ?? username ?? "no-reply@gestionproduccion.com";
            var fromName = _configuration["SMTP_FROM_NAME"] ?? "Gestão de Produção";

            // Support for SSL/TLS based on port
            // Port 465 requires SslOnConnect (Implicit SSL)
            // Port 587 requires StartTls (Explicit SSL)
            var secureOptions = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Bypass certificate validation if needed (common in some VPS environments)
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(host, port, secureOptions);
            
            if (!string.IsNullOrEmpty(username))
            {
                await client.AuthenticateAsync(username, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email successfully sent to {To} via {Host} (Port {Port}) using MailKit", to, host, port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MAILKIT ERROR: Failed to send email to {To}. Technical Detail: {Msg}", to, ex.Message);
        }
    }
}
