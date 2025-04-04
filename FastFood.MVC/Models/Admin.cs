﻿using Microsoft.AspNetCore.Identity;

namespace FastFood.MVC.Models
{
    public class Admin
    {
        public int AdminID { get; set; }

        public string UserID { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
