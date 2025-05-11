using FastFood.MVC.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FastFood.MVC.ViewModels
{
    public class MessageViewModel
    {
        public List<Message> Messages { get; set; } = new List<Message>();
        public Message? SelectedMessage { get; set; }
    }
    
    public class ReplyMessageViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Tên người gửi")]
        public string SenderName { get; set; } = null!;
        
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;
        
        [Display(Name = "Tin nhắn")]
        public string Content { get; set; } = null!;
        
        [Required(ErrorMessage = "Vui lòng nhập nội dung phản hồi")]
        [Display(Name = "Phản hồi")]
        public string Reply { get; set; } = null!;
    }
}
