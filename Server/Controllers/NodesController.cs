using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NetworkMonitor.Server.Models;
using NetworkMonitor.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyMongoNet;
using System.Linq;

namespace NetworkMonitor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class NodesController : BaseController
    {
        private readonly IMongoCollection<NetworkNode> nodesCol;

        public NodesController(IMongoCollection<NetworkNode> nodesCol, IMongoCollection<AuthUserX> usersCol) : base(usersCol)
        {
            this.nodesCol = nodesCol;
        }

        public async Task<ActionResult<List<NetworkNode>>> List()
        {
            return (await nodesCol.AllAsync()).ToList();
        }

        [HttpPost]
        public async Task<IActionResult> Save(List<NetworkNode> nodes)
        {
            foreach (var item in nodes)
            {
                if (item.Id == null)
                    await nodesCol.InsertOneAsync(item);
                else
                    await nodesCol.ReplaceOneAsync(n => n.Id == item.Id, item);
            }
            return Ok();
        }
    }
}
