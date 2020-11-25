using EasyMongoNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace NetworkMonitor.Server.Models
{
    public class AlertSendLog : MongoEntity
    {
        public string Destination { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string NodeId { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public string Response { get; set; }

        public string Error { get; set; }
    }
}
