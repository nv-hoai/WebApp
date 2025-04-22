using FastFood.MVC.Models;

namespace FastFood.MVC.ViewModels
{
    public record ProductIndexViewModel
    (
        IList<Category> Categories,
        IEnumerable<Product> Products
    );
}
