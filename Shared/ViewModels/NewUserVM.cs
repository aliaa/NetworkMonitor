using System.ComponentModel.DataAnnotations;

namespace NetworkMonitor.Shared.ViewModels
{
    public class NewUserVM
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(maximumLength: 20, MinimumLength = 5, ErrorMessage = "Username must be between 5 to 20 characters!")]
        public string Username { get; set; }

        [Required]
        [StringLength(maximumLength: 20, MinimumLength = 5, ErrorMessage = "Password must be between 5 to 20 characters!")]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Repeat of password does not match Password!")]
        [Display(Name = "Retype Password")]
        public string RetypePassword { get; set; }
    }
}
