using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FastFood.MVC.ViewModels
{
    public class PromotionViewModel
    {
        public int PromotionID { get; set; }

        public string? Description { get; set; }
        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        [StringLength(20, MinimumLength = 3)]
        public string Code { get; set; } = null!;

        [Display(Name = "Discount amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Expiry date")]
        public DateTime ExpiryDate { get; set; }
    }
}
