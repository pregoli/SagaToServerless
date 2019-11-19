using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SagaToServerless.Services
{
    public interface ISendGridService
    {
        Task SendEmail(string from, string subject, string to, string plainContent, string htmlContent);
    }
}
