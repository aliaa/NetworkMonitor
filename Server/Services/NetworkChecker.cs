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

        public NetworkChecker(IMongoCollection<NetworkNode> nodesCol, IMongoCollection<NodeStatusRange> statusCol, 
            IMongoCollection<GlobalAlertReceiver> alertReceiversCol,
            IHttpClientFactory httpClientFactory, IAlertSender alertSender, IConfiguration configuration)
        {
            this.nodesCol = nodesCol;
            this.statusCol = statusCol;
            this.alertReceiversCol = alertReceiversCol;
            this.httpClientFactory = httpClientFactory;
            this.alertSender = alertSender;
            this.alertContent = configuration.GetValue<string>("AlertContent");
            this.errorWaitSeconds = configuration.GetValue<int>("ErrorWaitSeconds");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
                var nodes = nodesCol.All();
                foreach (var n in nodes)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    var lastStatus = statusCol.Find(s => s.NodeId == n.Id).SortByDescending(s => s.LastTime).FirstOrDefault();

                    if (n.CheckMechanism == NetworkNode.CheckMechanismEnum.Ping)
                    {
                        Ping p = new Ping();
                        var reply = await p.SendPingAsync(n.Address);
                        if (lastStatus == null || lastStatus.IpStatus != reply.Status)
                            lastStatus = new NodeStatusRange { StartTime = DateTime.Now, NodeId = n.Id, IpStatus = reply.Status };
                    }
                    else if (n.CheckMechanism == NetworkNode.CheckMechanismEnum.Http)
                    {
                        var httpClient = httpClientFactory.CreateClient();
                        try
                        {
                            var reply = await httpClient.GetAsync(n.Address, stoppingToken);
                            if (lastStatus == null || lastStatus.HttpStatus != reply.StatusCode)
                                lastStatus = new NodeStatusRange { StartTime = DateTime.Now, NodeId = n.Id, HttpStatus = reply.StatusCode };
                        }
                        catch (Exception ex)
                        {
                            lastStatus.Error = ex;
                        }
                    }
                    else
                        throw new NotImplementedException();

                    if (!lastStatus.IsSuccess && !lastStatus.AlertSent && DateTime.Now - lastStatus.LastTime > TimeSpan.FromSeconds(errorWaitSeconds))
                    {
                        var content = alertContent.Replace("{nodeName}", n.Name);
                        if (n.AlertReceivers != null)
                            foreach (var dest in n.AlertReceivers)
                                await alertSender.Send(dest, content);
                        foreach (var dest in await alertReceiversCol.AllAsync())
                            await alertSender.Send(dest.Address, content);
                        lastStatus.AlertSent = true;
                    }

                    lastStatus.LastTime = DateTime.Now;
                    await statusCol.SaveAsync(lastStatus);
                }
            }
        }
    }
}
