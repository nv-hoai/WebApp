namespace FastFood.MVC.Models
{
    public class Promotion
    {
        public int PromotionID { get; set; }

        public string Code { get; set; } = null!;

        public decimal DiscountAmount { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
