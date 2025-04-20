using FastFood.MVC.Models;

namespace FastFood.MVC.ViewModels
{ 
    public record HomeViewModel(
        IList<Category> Categories,
        IList<Product> Carousels,
        IEnumerable<Product> Products,
        IEnumerable<Promotion> Promotions);
}
