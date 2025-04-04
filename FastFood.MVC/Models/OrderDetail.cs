using System.ComponentModel.DataAnnotations.Schema;

namespace FastFood.MVC.Models
{
    public class OrderDetail
    {
        public int OrderDetailID { get; set; }

        public int OrderID { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductID { get; set; }
        public Product Product { get; set; } = null!;

        public int? PromotionID { get; set; }
        public Promotion? Promotion { get; set; }

        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
    }
}
