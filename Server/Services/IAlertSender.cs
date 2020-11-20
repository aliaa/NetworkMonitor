using System.Threading.Tasks;

namespace NetworkMonitor.Server.Services
{
    public interface IAlertSender
    {
        Task<string> Send(string destination, string content);
    }
}
