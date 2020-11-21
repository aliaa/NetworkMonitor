using EasyMongoNet;

namespace NetworkMonitor.Shared.Models
{
    [CollectionIndex(new string[] { nameof(Address) })]
    public class GlobalAlertReceiver : MongoEntity
    {
        public string Address { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as GlobalAlertReceiver;
            if (other == null)
                return false;
            return other.Address == this.Address;
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }
    }
}
