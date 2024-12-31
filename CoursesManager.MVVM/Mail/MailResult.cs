using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CoursesManager.MVVM.Mail
{
    public class MailResult
    {
        public MailOutcome Outcome;
        public MailMessage MailMessage;
        public string? StudentName;
        public string? StudentEmail;
    }
}
