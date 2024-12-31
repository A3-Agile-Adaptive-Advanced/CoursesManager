using System;
using System.Net;
using System.Net.Mail;
using CoursesManager.MVVM.Env;
using CoursesManager.UI.Models;
using CoursesManager.MVVM.Mail;
using CoursesManager.MVVM.Mail.MailService;

namespace CoursesManager.UI.Service
{
    public class MailService : IMailService
    {
        public async Task<MailResult> SendMail(MailMessage mailMessage)
        {
            SmtpConfig _smtpConfig;

            try
            {
                // Haal SMTP connection string op
                var mailConnectionString = EnvManager<EnvModel>.Values.MailConnectionString;

                if (string.IsNullOrWhiteSpace(mailConnectionString))
                {
                    throw new Exception("MailConnectionString is niet ingesteld in .env");
                }

                _smtpConfig = ParseConnectionString(mailConnectionString);

                using var smtpClient = new SmtpClient(_smtpConfig.Server, _smtpConfig.Port)
                {
                    // Nodig om niet de basis authenticatie te gebruiken
                    UseDefaultCredentials = false,

                    // NetworkCrendential zorgt ervoor dat deze gegevens gebruikt worden voor het inloggen
                    Credentials = new NetworkCredential(_smtpConfig.User, _smtpConfig.Password),

                    // Moet altijd op true staan voor veilige verbinding
                    EnableSsl = true
                };
                // De user is een email en is ook de mail verstuurder
                MailAddress mailAddress = new MailAddress(_smtpConfig.User);
                mailMessage.From = mailAddress;
                await smtpClient.SendMailAsync(mailMessage);

                return new MailResult
                {
                    Outcome = MailOutcome.Success,
                    MailMessage = mailMessage
                };
            }
            catch (Exception ex)
            {
                return new MailResult
                {
                    Outcome = MailOutcome.Failure,
                    MailMessage = mailMessage
                };
            }
        }

        public async Task<List<MailResult>> SendMail(IEnumerable<MailMessage> mailMessages)
        {
            // Hergebruikt Sendmail functie om meerdere mails tegelijk te sturen, het wacht tot alle tasks klaar is.
            return (await Task.WhenAll(mailMessages.Select(mailMessage => SendMail(mailMessage)))).ToList();
        }

        private SmtpConfig ParseConnectionString(string connectionString)
        {
            var settings = ConnectionStringParser.Parse(connectionString);
            return new SmtpConfig
            {
                Server = settings["Server"],
                Port = int.Parse(settings["Port"]),
                User = settings["User"],
                Password = settings["Password"]
            };
        }

        private static class ConnectionStringParser
        {
            public static Dictionary<string, string> Parse(string connectionString)
            {
                var parameters = new Dictionary<string, string>();
                var pairs = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);

                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split('=', StringSplitOptions.RemoveEmptyEntries);
                    if (keyValue.Length == 2)
                    {
                        parameters[keyValue[0].Trim()] = keyValue[1].Trim();
                    }
                }

                return parameters;
            }
        }
    }
}