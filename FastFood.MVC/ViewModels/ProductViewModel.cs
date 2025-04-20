using FastFood.MVC.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFood.MVC.ViewModels
{
    public class ProductViewModel
    {
        public int ProductID { get; set; }
        public int CategoryID { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public IFormFile? ImageFile { get; set; }
        public int SoldQuantity { get; set; } = 0;
        public bool IsCarouselItem { get; set; } = false;
        public bool IsNew { get; set; } = false;
        public bool IsPopular { get; set; } = false;
    }
}
