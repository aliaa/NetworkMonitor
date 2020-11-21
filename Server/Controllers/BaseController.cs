using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NetworkMonitor.Server.Models;
using NetworkMonitor.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using EasyMongoNet;

namespace NetworkMonitor.Server.Controllers
{
    public class BaseController : ControllerBase
    {

        protected string Username => HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        protected string UserId => HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;

        protected IEnumerable<Permission> UserPermissions
        {
            get
            {
                if (User == null)
                    return Enumerable.Empty<Permission>();
                Claim claim = User.Claims.FirstOrDefault(c => c.Type == nameof(Permission));
                if (claim == null)
                    return Enumerable.Empty<Permission>();
                return claim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => (Permission)Enum.Parse(typeof(Permission), c));
            }
        }

    }
}
