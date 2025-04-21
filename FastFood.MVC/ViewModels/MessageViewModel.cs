using System.ComponentModel.DataAnnotations;

namespace FastFood.MVC.ViewModels
{
    public class MessageViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Tên")]
        public string SenderName { get; set; } = null!;

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = null!;

        [EmailAddress]

        public string Email { get; set; } = null!;

        [Display(Name = "Nội dung")]
        public string Message { get; set; } = null!;
    }
}
