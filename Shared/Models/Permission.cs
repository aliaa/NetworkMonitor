using System.ComponentModel.DataAnnotations;

namespace NetworkMonitor.Shared.Models
{
    public enum Permission
    {
        [Display(Name = "Define Nodes")]
        DefineNodes,
        [Display(Name = "Settings")]
        Settings,
        [Display(Name = "Manage Users")]
        ManageUsers
    }
}
