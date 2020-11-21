using NetworkMonitor.Shared.Models;

namespace NetworkMonitor.Shared.ViewModels
{
    public class NodeStatusVM : NodeStatusRange
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
