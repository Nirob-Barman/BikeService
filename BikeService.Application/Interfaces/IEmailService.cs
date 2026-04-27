using System.Net.Mail;

namespace BikeService.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string subject,
            string message,
            List<string> toEmails,
            List<string>? ccEmails = null,
            List<string>? bccEmails = null,
            List<Attachment>? attachments = null,
            bool isHtml = true,
            Dictionary<string, string>? templateVariables = null);
    }
}
