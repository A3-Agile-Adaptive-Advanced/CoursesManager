using System;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using CoursesManager.MVVM.Env;
using CoursesManager.UI.Models;
using CoursesManager.MVVM.Mail.MailService;
using CoursesManager.MVVM.Mail;
using ZstdSharp.Unsafe;
using System.ComponentModel.DataAnnotations;

public class MailService
{
    public async Task<MailResult> SendMail(MailMessage mailMessage)
    {
        SmtpConfig _smtpConfig;
       
        try
        {
            var mailConnectionString = EnvManager<EnvModel>.Values.MailConnectionString;

            if (string.IsNullOrWhiteSpace(mailConnectionString))
            {
                throw new Exception("MailConnectionString is niet ingesteld in .env");
            }

            _smtpConfig = ParseConnectionString(mailConnectionString);

            using var smtpClient = new SmtpClient(_smtpConfig.Server, _smtpConfig.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpConfig.User, _smtpConfig.Password),
                EnableSsl = true
            };
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
