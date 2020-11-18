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

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }
        
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        [Display(Name = "Checking Mechanism")]
        public CheckMechanismEnum CheckMechanism { get; set; }
    }
}
