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
using System.Net.Mail;
using System.Net;

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
            var fromName = _configuration["SMTP_FROM_NAME"] ?? "GestÃ£o de ProduÃ§Ã£o";

            using var client = new SmtpClient(host, port)
            {
                Credentials = !string.IsNullOrEmpty(username) ? new NetworkCredential(username, password) : null,
                EnableSsl = port == 587 || port == 465 || bool.Parse(_configuration["SMTP_SSL"] ?? "true")
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email successfully sent to {To} via {Host}", to, host);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}. Check your SMTP environment variables.", to);
        }
    }
}


