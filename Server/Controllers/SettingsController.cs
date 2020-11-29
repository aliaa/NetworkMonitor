using EasyMongoNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NetworkMonitor.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetworkMonitor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(nameof(Permission.DefineNodes))]
    public class SettingsController : BaseController
    {
        private readonly IMongoCollection<GlobalAlertReceiver> alertReceiversCol;

        public SettingsController(IMongoCollection<GlobalAlertReceiver> alertReceiversCol)
        {
            this.alertReceiversCol = alertReceiversCol;
        }

        public async Task<ActionResult<List<string>>> GlobalAlertReceivers()
        {
            return (await alertReceiversCol.AllAsync()).Select(a => a.Address).ToList();
        }

        [HttpPost]
        public async Task<IActionResult> SaveGlobalAlertReceivers(List<string> list)
        {
            await alertReceiversCol.DeleteManyAsync(_ => true);
            await alertReceiversCol.InsertManyAsync(list.Select(a => new GlobalAlertReceiver { Address = a }));
            return Ok();
        }
    }
}
