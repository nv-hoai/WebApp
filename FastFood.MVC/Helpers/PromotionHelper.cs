using Microsoft.Extensions.Logging.Abstractions;
using FastFood.MVC.Models;

namespace FastFood.MVC.Helpers
{
	public class PromotionHelper
	{
		public static decimal GetDiscountedPrice (decimal unitPrice, Promotion? promotion)
		{
			if (promotion == null || promotion.DiscountPercent <= 0)
			{
				return unitPrice;
			}	

			var discounted = unitPrice * (1 - promotion.DiscountPercent);

			if (promotion.MaximumDiscountAmount > 0)
			{
				var actualDiscount = unitPrice - discounted;
				if (actualDiscount > promotion.MaximumDiscountAmount)
				{
					discounted = unitPrice - promotion.MaximumDiscountAmount;
				}
			}	

			return discounted;
		}

		public static decimal GetSubtotal (decimal unitPrice, int quantity, Promotion? promotion)
		{
			var discountedPrice = GetDiscountedPrice(unitPrice, promotion);
			return discountedPrice * quantity;
		}
	}
}
