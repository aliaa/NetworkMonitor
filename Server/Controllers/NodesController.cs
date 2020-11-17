using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NetworkMonitor.Server.Models;
using NetworkMonitor.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpPost]
        public async Task<IActionResult> Save(List<NetworkNode> nodes)
        {
            await nodesCol.InsertManyAsync(nodes);
            return Ok();
        }
    }
}
