using System.ComponentModel.DataAnnotations.Schema;
using FastFood.MVC.Helpers;

namespace FastFood.MVC.Models
{
    public class OrderDetail
    {
        public int OrderID { get; set; }
        public virtual Order Order { get; set; } = null!;

        public int ProductID { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public Product? Product { get; set; } = null!;

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

		public void CalculatePrices()
		{
			DiscountedPrice = PromotionHelper.GetDiscountedPrice(UnitPrice, Promotion);
			SubTotal = PromotionHelper.GetSubtotal(UnitPrice, Quantity, Promotion);
		}
	}
}
