using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NetworkMonitor.Server.Services
{
    public class HttpGetAlertSender : IAlertSender
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string urlTemplate;
        private const string DESTINATION = "{destination}";
        private const string CONTENT = "{content}";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlTemplate">A URL containing {destination} and {content}.</param>
        public HttpGetAlertSender(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.urlTemplate = configuration.GetValue<string>("AlertServiceUri");
        }

        public async Task<string> Send(string destination, string content)
        {
            var url = new StringBuilder(urlTemplate).Replace(DESTINATION, destination).Replace(CONTENT, content).ToString();
            var httpClient = httpClientFactory.CreateClient();
            var result = await httpClient.GetStringAsync(url);
            return result;
        }
    }
}
