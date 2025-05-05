using System.ComponentModel.DataAnnotations.Schema;
using FastFood.MVC.Helpers;

namespace FastFood.MVC.Models
{
    public class CartItem
    {
		public int CustomerID { get; set; }
		public Customer Customer { get; set; } = null!;
		public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

		public int? PromotionID { get; set; }
		public string? PromotionName { get; set; }
		public Promotion? Promotion { get; set; }
		public decimal DiscountPercent { get; set; }
		public decimal DiscountedPrice { get; set; }
		public decimal SubTotal { get; set; }

		public void Calculate ()
		{
			DiscountPercent = Promotion?.DiscountPercent ?? 0;
			DiscountedPrice = PromotionHelper.GetDiscountedPrice(UnitPrice, Promotion);
			SubTotal = PromotionHelper.GetSubtotal(UnitPrice, Quantity, Promotion);
		}
		public DateTime CreatedAt { get; set; }
		public Product Product { get; set; } = null!;
	}
}
