using System;
using System.ComponentModel.DataAnnotations;

namespace FastFood.MVC.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        
        [Required]
        public string UserID { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string? Link { get; set; }
        
        public string? IconClass { get; set; } // Font Awesome class
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
