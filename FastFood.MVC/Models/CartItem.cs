using System.ComponentModel.DataAnnotations.Schema;
using FastFood.MVC.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FastFood.MVC.Models
{
    public class CartItem
    {
		public int CustomerID { get; set; }
		public virtual Customer Customer { get; set; } = null!;
		public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public virtual Product Product { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

		public int? PromotionID { get; set; }
		public string? PromotionName { get; set; }
		public virtual Promotion? Promotion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountedPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

		public void Calculate ()
		{
			DiscountedPrice = PromotionHelper.GetDiscountedPrice(UnitPrice, Promotion);
			SubTotal = PromotionHelper.GetSubtotal(UnitPrice, Quantity, Promotion);
		}
		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}
