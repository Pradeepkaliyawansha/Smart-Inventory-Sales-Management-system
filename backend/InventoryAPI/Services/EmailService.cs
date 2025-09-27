using System.Net;
using System.Net.Mail;
using System.Text;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Services.Interfaces;

namespace InventoryAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly bool _enableSsl;
        private readonly string _username;
        private readonly string _password;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
            _username = _configuration["EmailSettings:Username"] ?? "";
            _password = _configuration["EmailSettings:Password"] ?? "";
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    EnableSsl = _enableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_username, _password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_username, "Inventory Management System"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to: {Email}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to: {Email}", to);
                return false;
            }
        }

        public async Task<bool> SendStockAlertEmailAsync(List<StockAlertDto> stockAlerts)
        {
            try
            {
                if (!stockAlerts.Any())
                    return true;

                var subject = "Stock Alert - Low/Out of Stock Items";
                var body = GenerateStockAlertEmailBody(stockAlerts);

                var adminEmails = new[] { "admin@inventory.com" };

                foreach (var email in adminEmails)
                {
                    await SendEmailAsync(email, subject, body);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send stock alert email");
                return false;
            }
        }

        public async Task<bool> SendInvoiceEmailAsync(string customerEmail, InvoiceDto invoice)
        {
            try
            {
                var subject = $"Invoice {invoice.InvoiceNumber} - Inventory Management System";
                var body = GenerateInvoiceEmailBody(invoice);

                return await SendEmailAsync(customerEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invoice email to: {Email}", customerEmail);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken)
        {
            try
            {
                var subject = "Password Reset Request - Inventory Management System";
                var body = GeneratePasswordResetEmailBody(resetToken);

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to: {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string username, string temporaryPassword)
        {
            try
            {
                var subject = "Welcome to Inventory Management System";
                var body = GenerateWelcomeEmailBody(username, temporaryPassword);

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to: {Email}", email);
                return false;
            }
        }

        private string GenerateStockAlertEmailBody(List<StockAlertDto> stockAlerts)
        {
            var html = new StringBuilder();
            html.AppendLine("<html><body>");
            html.AppendLine("<h2>Stock Alert Notification</h2>");
            html.AppendLine("<p>The following products require attention:</p>");
            html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse;'>");
            html.AppendLine("<thead><tr style='background-color: #f2f2f2;'>");
            html.AppendLine("<th>Product Name</th><th>SKU</th><th>Current Stock</th><th>Min Stock Level</th><th>Alert Level</th>");
            html.AppendLine("</tr></thead><tbody>");

            foreach (var alert in stockAlerts)
            {
                var rowColor = alert.AlertLevel == "Critical" ? "#ffebee" : "#fff3e0";
                html.AppendLine($"<tr style='background-color: {rowColor};'>");
                html.AppendLine($"<td>{alert.ProductName}</td><td>{alert.SKU}</td><td>{alert.CurrentStock}</td>");
                html.AppendLine($"<td>{alert.MinStockLevel}</td><td><strong>{alert.AlertLevel}</strong></td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table>");
            html.AppendLine("<p>Please review and restock these items as necessary.</p>");
            html.AppendLine("<p>Best regards,<br/>Inventory Management System</p>");
            html.AppendLine("</body></html>");

            return html.ToString();
        }

        private string GenerateInvoiceEmailBody(InvoiceDto invoice)
        {
            var html = new StringBuilder();
            html.AppendLine("<html><body>");
            html.AppendLine("<h2>Invoice Details</h2>");
            html.AppendLine($"<p><strong>Invoice Number:</strong> {invoice.InvoiceNumber}</p>");
            html.AppendLine($"<p><strong>Date:</strong> {invoice.SaleDate:dd/MM/yyyy}</p>");
            html.AppendLine($"<p><strong>Customer:</strong> {invoice.Customer?.Name}</p>");
            html.AppendLine($"<p><strong>Total Amount:</strong> ${invoice.TotalAmount:F2}</p>");
            html.AppendLine("<p>Thank you for your business!</p>");
            html.AppendLine("<p>Best regards,<br/>Inventory Management System</p>");
            html.AppendLine("</body></html>");

            return html.ToString();
        }

        private string GeneratePasswordResetEmailBody(string resetToken)
        {
            var html = new StringBuilder();
            html.AppendLine("<html><body>");
            html.AppendLine("<h2>Password Reset Request</h2>");
            html.AppendLine("<p>You have requested to reset your password.</p>");
            html.AppendLine($"<p>Your temporary password is: <strong>{resetToken}</strong></p>");
            html.AppendLine("<p>Please use this to log in and change your password immediately.</p>");
            html.AppendLine("<p>Best regards,<br/>Inventory Management System</p>");
            html.AppendLine("</body></html>");

            return html.ToString();
        }

        private string GenerateWelcomeEmailBody(string username, string temporaryPassword)
        {
            var html = new StringBuilder();
            html.AppendLine("<html><body>");
            html.AppendLine("<h2>Welcome to Inventory Management System</h2>");
            html.AppendLine($"<p>Hello {username},</p>");
            html.AppendLine("<p>Your account has been created.</p>");
            html.AppendLine($"<p>Username: <strong>{username}</strong></p>");
            html.AppendLine($"<p>Temporary Password: <strong>{temporaryPassword}</strong></p>");
            html.AppendLine("<p>Please log in and change your password as soon as possible.</p>");
            html.AppendLine("<p>Best regards,<br/>Inventory Management System</p>");
            html.AppendLine("</body></html>");

            return html.ToString();
        }
    }
}