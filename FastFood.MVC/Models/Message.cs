using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFood.MVC.Models
{
    public enum MessageStatus
    {
        UnReplied,
        Replied
    }

    public class Message
    {
        public int Id { get; set; }

        [Display(Name = "Tên")]
        [Required(ErrorMessage = "Vui lòng nhập tên của bạn")]
        public string SenderName { get; set; } = null!;

        [Phone]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập nội dung tin nhắn")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = null!;

        [Display(Name = "Ngày gửi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái")]
        public MessageStatus Status { get; set; } = MessageStatus.UnReplied;

        [Display(Name = "Phản hồi")]
        public string? Reply { get; set; }

        [Display(Name = "Người trả lời")]
        public string? RepliedBy { get; set; }

        [Display(Name = "Ngày trả lời")]
        public DateTime? RepliedAt { get; set; }
    }
}
