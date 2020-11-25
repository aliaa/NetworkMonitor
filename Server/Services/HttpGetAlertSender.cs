using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using NetworkMonitor.Server.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NetworkMonitor.Server.Services
{
    public class HttpGetAlertSender : IAlertSender
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IMongoCollection<AlertSendLog> alertLogCol;
        private readonly string urlTemplate;
        private const string DESTINATION = "{destination}";
        private const string CONTENT = "{content}";

        /// <summary>
        /// Sends an HTTP GET request as an alert.
        /// </summary>
        /// <param name="urlTemplate">A URL containing {destination} and {content}.</param>
        public HttpGetAlertSender(IHttpClientFactory httpClientFactory, IConfiguration configuration, IMongoCollection<AlertSendLog> alertLogCol)
        {
            this.httpClientFactory = httpClientFactory;
            this.alertLogCol = alertLogCol;
            this.urlTemplate = configuration.GetValue<string>("AlertServiceUri");
        }

        public async Task<AlertSendLog> Send(string nodeId, string destination, string content)
        {
            var url = new StringBuilder(urlTemplate).Replace(DESTINATION, destination).Replace(CONTENT, content).ToString();
            var httpClient = httpClientFactory.CreateClient();
            var log = new AlertSendLog { NodeId = nodeId, Destination = destination };
            try
            {
                log.Response = await httpClient.GetStringAsync(url);
            }
            catch(Exception ex)
            {
                log.Error = GetErrorMessages(ex);
            }
            await alertLogCol.InsertOneAsync(log);
            return log;
        }

        private static string GetErrorMessages(Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            Exception ex = exception;
            while (ex != null)
            {
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.Append(ex.Message);
                ex = ex.InnerException;
            }
            return sb.ToString();
        }
    }
}
