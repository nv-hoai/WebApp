using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FastFood.MVC.ViewModels
{
    public class UserViewModel
    {
        [Display(Name = "STT")]
        public int Index { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "The passsword is required.")]
        [StringLength(100, ErrorMessage = "The password must be at least 6 and at max 100 characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "The password must be at least 6 and at max 100 characters long.")]
        [Display(Name = "Mật khẩu mới (Nếu muốn thay đổi)")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required]
        [Display(Name = "Vai trò")]
        public string RoleName { get; set; } = string.Empty;

        public SelectList Roles { get; set; } = new SelectList(new List<SelectListItem>
           {
               new SelectListItem { Text = "Khách hàng", Value = "Customer" },
               new SelectListItem { Text = "Nhân viên", Value = "Employee" },
               new SelectListItem { Text = "Shipper", Value = "Shipper" }
           }, "Value", "Text");
    }
}
