using Microsoft.AspNetCore.Identity;

namespace FastFood.MVC.Models
{
    public class Shipper
    {
        public int ShipperID { get; set; }

        public string UserID { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
