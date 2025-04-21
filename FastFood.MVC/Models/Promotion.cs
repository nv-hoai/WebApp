using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FastFood.MVC.Models
{
    public class Promotion
    {
        public int PromotionID { get; set; }
        [Display(Name = "Tên")]
        public string Name { get; set; } = null!;
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Ngày kết thúc")]
        public DateTime ExpiryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Phần trăm giảm")]
        [Range(0.0, 1.0, ErrorMessage = "The percent must between 0.0 and 1.0")]
        public decimal DiscountPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giới hạn tiền giảm")]
        public decimal MaximumDiscountAmount { get; set; } = 20.00m;
        
        public int? ProductID { get; set; }
        public virtual Product? Product { get; set; }
        public int? CategoryID { get; set; }
        public virtual Category? Category { get; set; }

        [Display(Name = "Số lượng mua cần thiết")]
        public int RequiredQuantity { get; set; } = 1;

        [JsonIgnore]
        public OrderDetail? OrderDetail { get; set; }
    }
}
