using System.Threading.Tasks;

namespace DemoBlog.Mvc.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
