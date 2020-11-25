using EasyMongoNet;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace NetworkMonitor.Shared.Models
{
    [CollectionIndex(new string[] { nameof(NodeId) })]
    public class NodeStatusRange : MongoEntity
    {
        public string NodeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastTime { get; set; }
        public DateTime? AlertSendTime { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public IPStatus? IpStatus { get; set; }
        public HttpStatusCode? HttpStatus { get; set; }
        public string ErrorMessage { get; set; }

        public bool IsSuccess
        {
            get
            {
                if (ErrorMessage != null)
                    return false;
                if (IpStatus != null)
                    return IpStatus == IPStatus.Success;
                else if (HttpStatus != null)
                    return (int)HttpStatus.Value / 100 == 2;
                return false;
            }
        }
    }
}
