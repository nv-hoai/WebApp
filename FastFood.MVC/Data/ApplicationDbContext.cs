using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FastFood.MVC.Models;
using System.Reflection.Metadata;

namespace FastFood.MVC.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Promotion> Promotions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>()
                .Property(c => c.LoyaltyPoint)
                .HasDefaultValue(0);

            modelBuilder.Entity<Order>()
                .Property(o => o.CreatedAt)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasDefaultValue("Unconfirmed");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalCharge)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderDetail>()
                .Property(o => o.Quantity)
                .HasDefaultValue(0);

            modelBuilder.Entity<OrderDetail>()
                .Property(o => o.SubTotal)
                .HasDefaultValue(0.0)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(o => o.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Promotion>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);
        }
    }
}
