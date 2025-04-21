using FastFood.MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FastFood.MVC.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, string adminEmail, string adminPassword)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

                await SeedRoles(roleManager);
                await SeedAdminUser(userManager, context, adminEmail, adminPassword);
                await SeedProduct(context);
                await SeedPromotion(context);
            }
        }

        private static async Task SeedRoles(RoleManager<ApplicationRole> roleManager)
        {
            string[] roleNames = { "Admin", "Customer", "Employee", "Shipper" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the roles and seed them to the database
                    await roleManager.CreateAsync(new ApplicationRole(roleName));
                }
            }
        }

        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager, ApplicationDbContext context, string email, string password)
        {
            var adminUser = await userManager.FindByEmailAsync(email);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, password);

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    var adminProfile = new Admin
                    {
                        UserID = adminUser.Id,
                        User = adminUser
                    };

                    context.Admins.Add(adminProfile);
                    await context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                var existingProfile = await context.Admins
                    .FirstOrDefaultAsync(a => a.UserID == adminUser.Id);

                if (existingProfile == null)
                {
                    context.Admins.Add(new Admin {
                        UserID = adminUser.Id,
                        User = adminUser
                    });
                    await context.SaveChangesAsync();
                }
            }
        }

        public static async Task SeedProduct(ApplicationDbContext context)
        {
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Burger và Sandwich" },
                    new Category { Name = "Gà rán" },
                    new Category { Name = "Combo" },
                    new Category { Name = "Đồ uống" }
                );
                await context.SaveChangesAsync();
            }
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product { 
                        Name = "Combo Burger 1",
                        Description = "Khuyến mãi hấp dẫn: Thưởng thức ngay Combo bao gồm burger hạng nặng, " +
                                    "2 lớp thịt bò, phô mai cheddar tan chảy, và khoai tây chiên giòn rụm.",
                        Price = 100.00m,
                        CategoryID = 3,
                        ImageUrl = "/img/carousel.jpeg",
                        IsCarouselItem = true
                    },
                    new Product {
                        Name = "Gà Rán Sốt Cay",
                        Description = "Gà rán giòn rụm, phủ sốt Habanero cay nồng, kết hợp với khoai tây chiên giòn" +
                                    " và sốt chấm hấp dẫn. Tăng độ cay cho những tín đồ ưa thích vị mạnh mẽ!",
                        Price = 100.00m,
                        CategoryID = 2,
                        ImageUrl = "/img/carousel-2.jpeg",
                        IsCarouselItem = true
                    },
                    new Product { 
                        Name = "Combo Mây Bay",
                        Description = "Món ăn bao gồm sandwich với lớp thịt xông khói, rau sống tươi ngon, kèm theo khoai " +
                                    "tây chiên giòn và sốt mayonnaise thơm béo. Một sự kết hợp hoàn hảo giữa " +
                                    "hương vị đậm đà và sự tươi mới, thích hợp cho bữa ăn nhanh và đầy đủ năng lượng.",
                        Price = 100.00m,
                        CategoryID = 3,
                        ImageUrl = "/img/carousel-3.jpeg",
                        IsCarouselItem = true
                    },
                    new Product
                    {
                        Name = "Classic Cheeseburger",
                        Description = "Gồm 1 Burger và phô mai tùy chọn",
                        Price = 70.00m,
                        CategoryID = 1,
                        ImageUrl = "/img/classic-cheeseburger.jpg",
                        IsPopular = true
                    },
                    new Product
                    {
                        Name = "Chicken Sandwich",
                        Description = "Gồm 1 sandwich gà giòn sốt mayo",
                        Price = 50.00m,
                        CategoryID = 1,
                        ImageUrl = "/img/chicken-sanwich.jpg"
                    },
                    new Product
                    {
                        Name = "Bacon Burger",
                        Description = "Gồm 1 Burger bacon nướng vàng và phô mai cheddar",
                        Price = 80.00m,
                        CategoryID = 1,
                        ImageUrl = "/img/bacon-burger.jpg"
                    },
                    new Product
                    {
                        Name = "Shrimp Burger",
                        Description = "Gồm 1 Burger tôm tươi và sốt mayo đặc biệt",
                        Price = 50.00m,
                        CategoryID = 1,
                        ImageUrl = "/img/shrimp-burger.jpg",
                        IsNew = true
                    },
                    new Product
                    {
                        Name = "Gà rán giòn",
                        Description = "Gồm 1 đùi gà rán giòn và khoai tây tặng kèm",
                        Price = 40.00m,
                        CategoryID = 2,
                        ImageUrl = "/img/crispy-fried-chicken.jpg"
                    },
                    new Product
                    {
                        Name = "Gà rán sốt cay",
                        Description = "Gồm 1 đùi gà rán sốt cay nhiều cấp độ và khoai tây tặng kèm",
                        Price = 45.00m,
                        CategoryID = 2,
                        ImageUrl = "/img/spicy-fried-chicken.jpg"
                    },
                    new Product
                    {
                        Name = "Gà viên Hàn Quốc",
                        Description = "Gồm 1 phần gà viên chiên giòn kết hợp 3 loại sốt chấm tùy chọn",
                        Price = 35.00m,
                        CategoryID = 2,
                        ImageUrl = "/img/chicken-nuggets.jpg"
                    },
                    new Product
                    {
                        Name = "Gà quay tiêu",
                        Description = "Gồm 1 miếng đùi gà quay tiêu đen và khoai tây tặng kèm",
                        Price = 75.00m,
                        CategoryID = 2,
                        ImageUrl = "/img/chicken-pepper.jpg"
                    },
                    new Product
                    {
                        Name = "Combo Couple Burger",
                        Description = "Gồm 2 burger truyền thống và 1 khoai tây chiên",
                        Price = 145.00m,
                        CategoryID = 3,
                        ImageUrl = "/img/combo-1.jpg"
                    },
                    new Product
                    {
                        Name = "Combo Couple Sandwich",
                        Description = "Gồm 2 sandwich gà giòn và 1 khoai tây chiên",
                        Price = 85.00m,
                        CategoryID = 3,
                        ImageUrl = "/img/combo-2.jpg"
                    },
                    new Product
                    {
                        Name = "Combo Triple",
                        Description = "Gồm 3 burger truyền thống, 1 phần gà sốt, 1 khoai tây chiên và 2 pepsi",
                        Price = 245.00m,
                        CategoryID = 3,
                        ImageUrl = "/img/combo-3.jpg"
                    },
                    new Product
                    {
                        Name = "Combo Family",
                        Description = "Gồm 4 burger truyền thống, 2 khoai tây chiên và 4 pepsi",
                        Price = 279.00m,
                        CategoryID = 3,
                        ImageUrl = "/img/combo-4.jpg"
                    },
                    new Product
                    {
                        Name = "Pepsi",
                        Description = "Nước ngọt Pepsi lon 330ml",
                        Price = 15.00m,
                        CategoryID = 4,
                        ImageUrl = "/img/pepsi.jpg"
                    },
                    new Product
                    {
                        Name = "7Up",
                        Description = "Nước ngọt 7Up lon 330ml",
                        Price = 15.00m,
                        CategoryID = 4,
                        ImageUrl = "/img/7up.jpg"
                    },
                    new Product
                    {
                        Name = "Trà xanh không độ",
                        Description = "Chai trà xanh không độ 500ml",
                        Price = 20.00m,
                        CategoryID = 4,
                        ImageUrl = "/img/green-tea.jpg"
                    },
                    new Product
                    {
                        Name = "Combo Gà Rán + Khoai Tây Chiên",
                        Description = "Gồm 1 gà rán truyền thống và 1 khoai tây chiên",
                        Price = 80.00m,
                        CategoryID = 3,
                        ImageUrl = "/img/fried-chicken-and-french-fries.jpg"
                    }
                );
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedPromotion(ApplicationDbContext context)
        {
            if (!context.Promotions.Any())
            {
                context.Promotions.AddRange(
                    new Promotion
                    {
                        Name = "Combo Gà Rán + Khoai Tây Chiên",
                        Description = "Giảm 20% cho combo gà rán và khoai tây chiên. Khi mua trực tiếp tại cửa hàng!",
                        StartDate = DateTime.Now,
                        ExpiryDate = DateTime.Now.AddDays(30),
                        DiscountPercent = 0.2m,
                        ProductID = context.Products.Where(p => p.Name.Equals("Combo Gà Rán + Khoai Tây Chiên")).Select(p => p.ProductID).FirstOrDefault()
                    },
                    new Promotion
                    {
                        Name = "BurgerSandwitch & Soft Drinks Combo",
                        Description = "Mua một burger hoặc một sandwitch tặng một ly nước giải khát miễn phí. Khuyến mãi chỉ áp dụng trong tuần này.",
                        StartDate = DateTime.Now,
                        ExpiryDate = DateTime.Now.AddDays(15),
                        DiscountPercent = 0,
                        CategoryID = 1
                    },
                    new Promotion
                    {
                        Name = "Gà Tẩm Sốt Mới - Giảm 15%",
                        Description = "Thử ngay món gà tẩm sốt mới lạ, giảm giá 15% cho tất cả các đơn hàng có món gà tẩm sốt.",
                        StartDate = DateTime.Now,
                        ExpiryDate = DateTime.Now.AddDays(15),
                        DiscountPercent = 0.15m,
                        ProductID = context.Products.Where(p => p.Name.Equals("Gà rán sốt cay")).Select(p => p.ProductID).FirstOrDefault()
                    },
                    new Promotion
                    {
                        Name = "Combo Đặc Biệt Cho Gia Đình",
                        Description = "Mua 2 combo gia đình, tặng thêm một phần khoai tây chiên size lớn miễn phí!",
                        StartDate = DateTime.Now,
                        ExpiryDate = DateTime.Now.AddDays(15),
                        DiscountPercent = 0,
                        ProductID = context.Products.Where(p => p.Name.Equals("Combo Family")).Select(p => p.ProductID).FirstOrDefault()
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
