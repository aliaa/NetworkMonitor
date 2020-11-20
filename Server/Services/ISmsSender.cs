using System.Threading.Tasks;

namespace NetworkMonitor.Server.Services
{
    public interface ISmsSender
    {
        Task<string> Send(string destination, string content);
    }
}
