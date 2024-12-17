using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CoursesManager.MVVM.Mail.MailService
{
    public interface IMailService
    {
        Task<MailResult> SendMail(MailMessage mailMessage);
        Task<List<MailResult>> SendMail(IEnumerable<MailMessage> mailMessages);
    }
}
