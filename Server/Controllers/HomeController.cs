using EasyMongoNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NetworkMonitor.Server.Models;
using NetworkMonitor.Shared.Models;
using NetworkMonitor.Shared.ViewModels;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetworkMonitor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly IMongoCollection<NodeStatusRange> nodeStatusCol;
        private readonly IMongoCollection<NetworkNode> nodesCol;

        public HomeController(IMongoCollection<NodeStatusRange> nodeStatusCol, IMongoCollection<NetworkNode> nodesCol)
        {
            this.nodeStatusCol = nodeStatusCol;
            this.nodesCol = nodesCol;
        }

        public async Task<ActionResult<List<NodeStatusVM>>> Status()
        {
            //var lastStatus = (await nodeStatusCol.Aggregate()
            //    .SortByDescending(s => s.LastTime)
            //    .Group(id => id.NodeId, g => new { First = g.FirstOrDefault() })
            //    .ToListAsync())
            //    .Select(a => Mapper.Map<NodeStatusVM>(a.First))
            //    .ToList();

            var lastStatus = nodeStatusCol.AsQueryable()
                .OrderByDescending(s => s.LastTime)
                .GroupBy(s => s.NodeId)
                .ToList()
                .Select(g => Mapper.Map<NodeStatusVM>(g.First(), null))
                .ToList();

            var nodes = (await nodesCol.AllAsync()).ToDictionary(k => k.Id);
            foreach (var item in lastStatus)
            {
                if (nodes.ContainsKey(item.NodeId))
                {
                    item.Address = nodes[item.NodeId].Address;
                    item.Name = nodes[item.NodeId].Name;
                }
            }
            return lastStatus.Where(i => i.Name != null).ToList();
        }
    }
}
