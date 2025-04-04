using Microsoft.AspNetCore.Identity;

namespace FastFood.MVC.Models
{
    public class Shipper
    {
        public int ShipperID { get; set; }

        public string UserID { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;
    }
}
