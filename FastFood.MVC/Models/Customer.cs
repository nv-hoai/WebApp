using Microsoft.AspNetCore.Identity;

namespace FastFood.MVC.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }

        public string UserID { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        public int? AddressID { get; set; }
        public virtual Address? Address { get; set; }

        public int LoyaltyPoint { get; set; }
    }
}
