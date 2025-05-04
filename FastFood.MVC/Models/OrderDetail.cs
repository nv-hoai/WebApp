using System.ComponentModel.DataAnnotations.Schema;

namespace FastFood.MVC.Models
{
    public class OrderDetail
    {
        public int OrderID { get; set; }
        public virtual Order Order { get; set; } = null!;

        public int ProductID { get; set; }
        public virtual Product Product { get; set; } = null!;
		[Column(TypeName = "decimal(18,2)")]
		public decimal UnitPrice { get; set; }

		public int? PromotionID { get; set; }
        public virtual Promotion? Promotion { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal DiscountedUnitPrice { get; set; }
		public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

		public void CalculateSubTotal()
		{
			if (Promotion != null && Promotion.DiscountPercent > 0)
			{
				var discounted = UnitPrice * (1 - Promotion.DiscountPercent);

				if (Promotion.MaximumDiscountAmount > 0)
				{
					var maxDiscount = Promotion.MaximumDiscountAmount;
					var actualDiscount = UnitPrice - discounted;

					if (actualDiscount > maxDiscount)
						discounted = UnitPrice - maxDiscount;
				}

				DiscountedUnitPrice = discounted;
			}
			else
			{
				DiscountedUnitPrice = UnitPrice;
			}

			SubTotal = Quantity * DiscountedUnitPrice;
		}

	}
}
