using EasyMongoNet;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace NetworkMonitor.Shared.Models
{
    public class NetworkNode : MongoEntity
    {
        public enum CheckMechanismEnum
        {
            Ping,
            Http
        }

        public string Name { get; set; }
        public string Address { get; set; }
        
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        [Display(Name = "Checking Mechanism")]
        public CheckMechanismEnum CheckMechanism { get; set; }
    }
}
