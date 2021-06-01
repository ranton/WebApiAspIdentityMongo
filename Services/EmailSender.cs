using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POC_WebApiAspIdentityMongo.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            Console.WriteLine("{0}", email);
        }
    }
}
