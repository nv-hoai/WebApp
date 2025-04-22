using FastFood.MVC.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFood.MVC.ViewModels
{
    public class ProductCreateViewModel
    {
        public int ProductID { get; set; }
        public int CategoryID { get; set; }

        [Display(Name = "Tên")]
        public string Name { get; set; } = null!;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Display(Name = "Ảnh")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Số lượng đã bán")]
        public int SoldQuantity { get; set; } = 0;

        [Display(Name = "Dành cho quảng cáo")]
        public bool IsCarouselItem { get; set; } = false;

        [Display(Name = "Sản phẩm mới")]
        public bool IsNew { get; set; } = false;

        [Display(Name = "Sản phẩm phổ biến")]
        public bool IsPopular { get; set; } = false;
    }
}
