using CheckLogServer.Models;
using CheckLogUtility.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CheckLogServer.Services
{
    public class EmailService
    {
        public EmailService(DatabaseContext database) => this.database = database;
        private readonly DatabaseContext database;

        public async Task<bool> SendMessageAsync(string subject, Func<Email, string> body)
        {
            var emails = await database.Emails.ToListAsync();
            if (emails.Count <= 0)
            {
                return false;
            }

            foreach (var email in emails)
            {
                using var client = new SmtpClient(email.Host, email.Port);

                using var message = new MailMessage()
                {
                    From = new MailAddress(email.From),
                    Subject = $"{email.SubjectTag} {subject}",
                    Body = body(email),
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(email.To));

                try
                {
                    await client.SendMailAsync(message);
                    return true;
                }
                catch (Exception) { }
            }

            return false;
        }
    }
}
