using System.ComponentModel.DataAnnotations.Schema;

namespace FastFood.MVC.Models
{
    public class OrderDetail
    {
        public int OrderID { get; set; }
        public virtual Order Order { get; set; } = null!;

        public int ProductID { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int? PromotionID { get; set; }
        public virtual Promotion? Promotion { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }
    }
}
