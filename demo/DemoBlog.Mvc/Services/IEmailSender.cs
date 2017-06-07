using System.Threading.Tasks;

namespace DemoBlog.Mvc.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
