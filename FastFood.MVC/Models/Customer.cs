using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace FastFood.MVC.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }

        public string UserID { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        [JsonIgnore]
        public virtual Address? Address { get; set; }

        public int LoyaltyPoint { get; set; } = 0;
		public virtual ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
		public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
