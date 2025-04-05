using FastFood.MVC.Models;
using Microsoft.AspNetCore.Identity;
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
                await SeedAdminUser(userManager, adminEmail, adminPassword);
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

        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager, string email, string password)
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
                }
                else
                {
                    throw new Exception($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
