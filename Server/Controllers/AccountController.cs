using EasyMongoNet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NetworkMonitor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IMongoCollection<AuthUserX> userCol;
        public AccountController(IMongoCollection<AuthUserX> userCol) 
        {
            this.userCol = userCol;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ClientAuthUser>> Login(LoginVM model)
        {
            if (model == null)
                return Unauthorized();
            var user = userCol.CheckAuthentication(model.Username, model.Password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, model.Username),
                    new Claim(ClaimTypes.Name, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName)
                };
                if (user.IsAdmin)
                    claims.Add(new Claim("IsAdmin", "true"));

                IEnumerable<Permission> permissions;
                if (user.IsAdmin)
                    permissions = Enum.GetValues<Permission>();
                else
                    permissions = user.Permissions;
                var perms = new StringBuilder();
                foreach (var perm in permissions)
                    perms.Append(perm).Append(',');
                claims.Add(new Claim(nameof(Permission), perms.ToString()));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                var clientUser = Mapper.Map<ClientAuthUser>(user);
                return clientUser;
            }
            return Unauthorized();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new AuthenticationProperties { IsPersistent = false });
            return Ok();
        }

        [Authorize(nameof(Permission.ManageUsers))]
        [HttpPost]
        public IActionResult Add(NewUserVM user)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (userCol.Any(u => u.Username == user.Username))
                return BadRequest(new Dictionary<string, List<string>> { { nameof(NewUserVM.Username), new List<string> { "Username already exists!" } } });
            var authUser = Mapper.Map<AuthUserX>(user);
            userCol.Save(authUser);
            return Ok();
        }

        [Authorize(nameof(Permission.ManageUsers))]
        public ActionResult<List<ClientAuthUser>> List()
        {
            return userCol.Find(u => u.Disabled != true).SortBy(u => u.LastName).ThenBy(u => u.FirstName)
                .Project(Builders<AuthUserX>.Projection.Exclude(u => u.HashedPassword)).As<AuthUserX>()
                .ToEnumerable().Select(u => Mapper.Map<ClientAuthUser>(u)).ToList();
        }

        [Authorize(nameof(Permission.ManageUsers))]
        [HttpPost]
        public IActionResult Save(ClientAuthUser user)
        {
            if (!ModelState.IsValid)
                return BadRequest("User data is invalid!");
            var existing = userCol.FindById(user.Id);
            if (existing == null)
                return BadRequest("User not found!");
            existing.InjectFrom(user);
            userCol.Save(existing);
            return Ok();
        }
    }
}
