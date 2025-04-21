using System.ComponentModel.DataAnnotations;

namespace FastFood.MVC.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        [Display(Name = "Tên")]
        public string Name { get; set; } = null!;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
        public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    }
}
