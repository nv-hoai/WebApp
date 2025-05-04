namespace FastFood.MVC.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
		public int? PromotionID { get; set; }
		public string? PromotionName { get; set; } 
		public decimal DiscountPercent { get; set; } = 0;

		public decimal SubTotal => CalculateSubTotal();

		public decimal CalculateSubTotal()
		{
			return Price * Quantity * (1 - DiscountPercent);
		}
	}
}
