using NetworkMonitor.Server.Models;
using System.Threading.Tasks;

namespace NetworkMonitor.Server.Services
{
    public interface IAlertSender
    {
        Task<AlertSendLog> Send(string nodeId, string destination, string content);
    }
}
