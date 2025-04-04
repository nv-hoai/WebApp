namespace FastFood.MVC.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public string? Image { get; set; }
    }
}
