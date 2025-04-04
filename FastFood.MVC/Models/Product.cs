﻿
namespace FastFood.MVC.Models
{
    public class Product
    {
        public int ProductID { get; set; }

        public int CategoryID { get; set; }
        public virtual Category Category { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
    }
}
