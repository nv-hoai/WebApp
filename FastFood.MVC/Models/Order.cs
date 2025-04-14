using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FastFood.MVC.Models
{
    public enum OrderStatus { Pending, Processing, Completed, Delivering, Cancelled }
    public class Order
    {
        public int OrderID { get; set; }

        public int CustomerID { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        public int? ShipperID { get; set; }
        public virtual Shipper? Shipper { get; set; }

        public int? EmployeeID { get; set; }
        public virtual Employee? Employee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalCharge { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();

        public void CalculateTotalCharge()
        {
            TotalCharge = OrderDetails.Sum(od => od.SubTotal);
        }
    }
}
