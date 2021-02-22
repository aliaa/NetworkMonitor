using EasyMongoNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using NetworkMonitor.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkMonitor.Server.Services
{
    public class NetworkChecker : BackgroundService
    {
        private readonly IMongoCollection<NetworkNode> nodesCol;
        private readonly IMongoCollection<NodeStatusRange> statusCol;
        private readonly IMongoCollection<GlobalAlertReceiver> alertReceiversCol;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IAlertSender alertSender;
        private readonly string alertContent;
        private readonly int errorWaitSeconds;
        private readonly int alertResendMinutes;

        public NetworkChecker(IMongoCollection<NetworkNode> nodesCol, IMongoCollection<NodeStatusRange> statusCol,
            IMongoCollection<GlobalAlertReceiver> alertReceiversCol,
            IHttpClientFactory httpClientFactory, IAlertSender alertSender, IConfiguration configuration)
        {
            this.nodesCol = nodesCol;
            this.statusCol = statusCol;
            this.alertReceiversCol = alertReceiversCol;
            this.httpClientFactory = httpClientFactory;
            this.alertSender = alertSender;
            this.alertContent = configuration.GetValue("AlertContent", "{nodeName} is not working!");
            this.errorWaitSeconds = configuration.GetValue("ErrorWaitSeconds", 120);
            this.alertResendMinutes = configuration.GetValue("AlertResendMinutes", 1440);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
                var nodes = nodesCol.All();
                foreach (var node in nodes)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    var lastStatus = statusCol.Find(s => s.NodeId == node.Id).SortByDescending(s => s.LastTime).FirstOrDefault(cancellationToken: stoppingToken);

                    if (node.CheckMechanism == NetworkNode.CheckMechanismEnum.Ping)
                    {
                        Ping p = new Ping();
                        try
                        {
                            if (lastStatus == null)
                                lastStatus = NewStatus(node.Id);
                            var reply = await p.SendPingAsync(node.Address, 2000);
                            if (lastStatus.IpStatus != reply.Status)
                                lastStatus = NewStatus(node.Id);
                            lastStatus.IpStatus = reply.Status;
                        }
                        catch (Exception ex)
                        {
                            lastStatus.ErrorMessage = GetErrorMessages(ex);
                        }
                    }
                    else if (node.CheckMechanism == NetworkNode.CheckMechanismEnum.Http)
                    {
                        var httpClient = httpClientFactory.CreateClient();
                        try
                        {
                            string address = node.Address;
                            if (!address.StartsWith("http"))
                                address = "http://" + node.Address;
                            if (lastStatus == null)
                                lastStatus = NewStatus(node.Id);
                            var reply = await httpClient.GetAsync(address, stoppingToken);
                            if (lastStatus.HttpStatus != reply.StatusCode)
                                lastStatus = NewStatus(node.Id);
                            lastStatus.HttpStatus = reply.StatusCode;
                        }
                        catch (Exception ex)
                        {
                            lastStatus.ErrorMessage = GetErrorMessages(ex);
                        }
                    }
                    else
                        throw new NotImplementedException();

                    // Send alert if required:

                    if (!lastStatus.IsSuccess 
                        && (lastStatus.AlertSendTime == null || DateTime.Now - lastStatus.AlertSendTime > TimeSpan.FromMinutes(alertResendMinutes)) 
                        && DateTime.Now - lastStatus.StartTime > TimeSpan.FromSeconds(errorWaitSeconds))
                    {
                        var content = alertContent.Replace("{nodeName}", node.Name);
                        var receivers = new HashSet<string>();
                        if (node.AlertReceivers != null)
                            foreach (var dest in node.AlertReceivers)
                            {
                                if (!receivers.Contains(dest))
                                {
                                    await alertSender.Send(node.Id, dest, content);
                                    receivers.Add(dest);
                                }
                            }
                        foreach (var dest in (await alertReceiversCol.AllAsync()).Select(r => r.Address))
                        {
                            if (!receivers.Contains(dest))
                            {
                                await alertSender.Send(node.Id, dest, content);
                                receivers.Add(dest);
                            }
                        }
                        lastStatus.AlertSendTime = DateTime.Now;
                    }

                    lastStatus.LastTime = DateTime.Now;
                    await statusCol.SaveAsync(lastStatus);
                }
            }
        }

        private static NodeStatusRange NewStatus(string nodeId)
        { 
            return new NodeStatusRange { StartTime = DateTime.Now, NodeId = nodeId, LastTime = DateTime.Now };
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
