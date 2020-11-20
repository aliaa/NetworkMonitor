using EasyMongoNet;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace NetworkMonitor.Shared.Models
{
    [CollectionIndex(new string[] { nameof(Name) }, Unique = true)]
    [CollectionIndex(new string[] { nameof(Address) }, Unique = true)]
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

        [Display(Name = "Specific Alert Receivers")]
        public string[] AlertReceivers { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public string AlertReceiversText
        {
            get
            {
                if (AlertReceivers == null)
                    return "";
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < AlertReceivers.Length; i++)
                {
                    sb.Append(AlertReceivers[i]);
                    if (i < AlertReceivers.Length - 1)
                        sb.Append('\n');
                }
                return sb.ToString();
            }
            set
            {
                AlertReceivers = value.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
