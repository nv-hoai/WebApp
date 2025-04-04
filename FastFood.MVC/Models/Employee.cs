using Microsoft.AspNetCore.Identity;

namespace FastFood.MVC.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }

        public string UserID { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;
    }
}
