//using FastFood.MVC.Models;
//using FastFood.MVC.Data;
//using Microsoft.EntityFrameworkCore;

//namespace FastFood.MVC.Services
//{
//    public class PromotionService
//    {
//        public async Task<IEnumerable<Promotion>> GetPromotions(ApplicationDbContext context, OrderDetail orderDetail)
//        {
//            var promotions = await context.Promotions
//                .Where(p => (p.StartDate <= DateTime.Now && p.ExpiryDate > DateTime.Now)
//                        && (p.ProductID == orderDetail.ProductID 
//                        || p.CategoryID == orderDetail.Product.CategoryID))
//                .ToListAsync();
//            return promotions;
//        }
//    }
//}
