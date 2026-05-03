using CleanArchitecture.Core.DTOs.Email;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        public MailSettings _mailSettings { get; }
        public ILogger<EmailService> _logger { get; }
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger, IHttpClientFactory httpClientFactory)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendAsync(EmailRequest request)
        {
            if (!string.IsNullOrWhiteSpace(_mailSettings.ApiKey))
                await SendViaBrevoApiAsync(request);
            else
                await SendViaSmtpAsync(request);
        }

        private async Task SendViaBrevoApiAsync(EmailRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("api-key", _mailSettings.ApiKey);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var from = request.From ?? _mailSettings.From;
                var displayName = _mailSettings.DisplayName ?? "UniNexus";

                var payload = new
                {
                    sender = new { name = displayName, email = from },
                    to = new[] { new { email = request.To } },
                    subject = request.Subject,
                    htmlContent = request.Body
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Brevo API error: {Status} {Body}", response.StatusCode, responseBody);
                    throw new ApiException($"Email could not be sent: {responseBody}");
                }

                _logger.LogInformation("Email sent via Brevo API to {To}", request.To);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Email could not be sent via Brevo: {Message}", ex.Message);
                throw new ApiException(ex.Message);
            }
        }

        private async Task SendViaSmtpAsync(EmailRequest request)
        {
            try
            {
                var from = request.From ?? _mailSettings.From;
                var displayName = _mailSettings.DisplayName ?? "UniNexus";

                using var message = new MailMessage
                {
                    From = new MailAddress(from, displayName),
                    Subject = request.Subject,
                    Body = request.Body,
                    IsBodyHtml = true,
                };
                message.To.Add(request.To);

                using var smtp = new SmtpClient(_mailSettings.Host, _mailSettings.Port)
                {
                    Credentials = new NetworkCredential(_mailSettings.UserName, _mailSettings.Password),
                    EnableSsl = true,
                };

                await smtp.SendMailAsync(message);
                _logger.LogInformation("Email sent via SMTP to {To}", request.To);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Email could not be sent via SMTP: {Message}", ex.Message);
                throw new ApiException(ex.Message);
            }
        }
    }
}
