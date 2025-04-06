using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FastFood.MVC.Models
{
    public class Promotion
    {
        public int PromotionID { get; set; }

        [StringLength(20, MinimumLength = 3)]
        public string Code { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }
        public DateTime ExpiryDate { get; set; }

        [JsonIgnore]
        public OrderDetail? OrderDetail { get; set; }
    }
}
