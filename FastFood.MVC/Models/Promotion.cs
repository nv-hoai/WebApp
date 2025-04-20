using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FastFood.MVC.Models
{
    public class Promotion
    {
        public int PromotionID { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Expiry date")]
        public DateTime ExpiryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Discount percent")]
        [Range(0.0, 1.0, ErrorMessage = "The percent must between 0.0 and 1.0")]
        public decimal DiscountPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Maximum discount amount")]
        public decimal MaximumDiscountAmount { get; set; } = 20.00m;
        
        public int? ProductID { get; set; }
        public virtual Product? Product { get; set; }
        public int? CategoryID { get; set; }
        public virtual Category? Category { get; set; }

        [Display(Name = "Required quantity")]
        public int RequiredQuantity { get; set; } = 1;

        [JsonIgnore]
        public OrderDetail? OrderDetail { get; set; }
    }
}
