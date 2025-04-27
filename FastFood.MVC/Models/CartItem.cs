namespace FastFood.MVC.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal => Price * Quantity;
        public int? PromotionID { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal CalculateSubTotal ()
        {
            return (PromotionID.HasValue ? 
                    Price * Quantity * (1 - DiscountPercent) : 
                    Price * Quantity);
        }
    }
}
