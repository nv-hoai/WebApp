using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FastFood.MVC.ViewModels.Account
{
    public class UserViewModel
    {

        public int Index { get; set; }

        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; } = string.Empty;

        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>
            {
                new SelectListItem { Text = "Customer", Value = "Customer" },
                new SelectListItem { Text = "Employee", Value = "Employee" },
                new SelectListItem { Text = "Shipper", Value = "Shipper" }
            };
    }
}
