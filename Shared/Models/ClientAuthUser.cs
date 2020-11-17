using EasyMongoNet;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetworkMonitor.Shared.Models
{
    public class ClientAuthUser : MongoEntity
    {
        [Required]
        public string Username { get; set; }

        public bool IsAdmin { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public bool Disabled { get; set; }

        [Display(Name = "Display Name")]
        public string DisplayName
        {
            get { return FirstName + " " + LastName; }
        }

        public List<Permission> Permissions { get; set; } = new List<Permission>();

        public bool HasPermission(Permission perm)
        {
            return IsAdmin || Permissions.Contains(perm);
        }
    }
}
