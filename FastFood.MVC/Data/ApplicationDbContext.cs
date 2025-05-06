using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FastFood.MVC.Models;
using System.Reflection.Metadata;

namespace FastFood.MVC.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<
            ApplicationUser, ApplicationRole, string,
            ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin,
            ApplicationRoleClaim, ApplicationUserToken>
    {
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Admin>()
                .HasOne(c => c.User)
                .WithOne(a => a.Admin)
                .HasForeignKey<Admin>(a => a.UserID)
                .OnDelete(DeleteBehavior.Cascade); // Delete Admin if User is deleted


            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserID)
                .OnDelete(DeleteBehavior.Cascade); // Delete Customer if User is deleted


            modelBuilder.Entity<Employee>()
                .HasOne(c => c.User)
                .WithOne(e => e.Employee)
                .HasForeignKey<Employee>(e => e.UserID)
                .OnDelete(DeleteBehavior.Cascade); // Delete Employee if User is deleted


            modelBuilder.Entity<Shipper>()
                .HasOne(c => c.User)
                .WithOne(s => s.Shipper)
                .HasForeignKey<Shipper>(s => s.UserID)
                .OnDelete(DeleteBehavior.Cascade); // Delete Shipper if User is deleted

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of Category if it has Products

            modelBuilder.Entity<Product>()
                .ToTable(t => t.HasCheckConstraint("CK_Product_Price", "[Price] >= 0"));

            modelBuilder.Entity<Order>(o =>
            {
                o.HasOne(o => o.Customer)
                    .WithMany(e => e.Orders)
                    .HasForeignKey(o => o.CustomerID)
                    .OnDelete(DeleteBehavior.NoAction); // Should be manually handled, when cusomter is deleted, orders should be archived or deleted based on business logic

                o.HasOne(o => o.Employee)
                    .WithMany(e => e.Orders)
                    .HasForeignKey(o => o.EmployeeID)
                    .OnDelete(DeleteBehavior.NoAction);

                o.HasOne(o => o.Shipper)
                    .WithMany(s => s.Orders)
                    .HasForeignKey(o => o.ShipperID)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            modelBuilder.Entity<OrderDetail>(od =>
            {
                od.HasKey(od => new { od.OrderID, od.ProductID }); // Composite key

                od.HasOne(od => od.Order)
                    .WithMany(o => o.OrderDetails)
                    .HasForeignKey(od => od.OrderID)
                    .OnDelete(DeleteBehavior.Cascade); // Delete OrderDetails if Order is deleted

                od.HasOne(od => od.Promotion)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(od => od.PromotionID)
                    .OnDelete(DeleteBehavior.NoAction); // Promotions are reusable, avoid cascade
            });

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne(e => e.User)
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne(e => e.User)
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<ApplicationRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                // Each Role can have many associated RoleClaims
                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });

            modelBuilder.Entity<CartItem>(c =>
            {
                c.HasKey(ci => new { ci.CustomerID, ci.ProductID });

                c.HasOne(ci => ci.Customer)
                    .WithMany(c => c.CartItems)
                    .HasForeignKey(ci => ci.CustomerID)
                    .OnDelete(DeleteBehavior.Cascade);

                c.HasOne(ci => ci.Product)
                    .WithMany()
                    .HasForeignKey(ci => ci.ProductID)
                    .OnDelete(DeleteBehavior.Restrict);

                c.HasOne(ci => ci.Promotion)
                    .WithMany()
                    .HasForeignKey(ci => ci.PromotionID)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
