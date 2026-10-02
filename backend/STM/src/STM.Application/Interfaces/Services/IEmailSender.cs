using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Application.Interfaces.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string body);
    }

}
