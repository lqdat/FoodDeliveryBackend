using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using FoodDeliveryBackend.Core.Entities;

namespace FoodDeliveryBackend.Infrastructure.Data;

public class FoodDeliveryDbContext : DbContext
{
    public FoodDeliveryDbContext(DbContextOptions<FoodDeliveryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<Cart> Carts { get; set; }
    public virtual DbSet<CartItem> CartItems { get; set; }
    public virtual DbSet<ChatMessage> ChatMessages { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<Driver> Drivers { get; set; }
    public virtual DbSet<DriverEarning> DriverEarnings { get; set; }
    public virtual DbSet<FoodCategory> FoodCategories { get; set; }
    public virtual DbSet<MenuCategory> MenuCategories { get; set; }
    public virtual DbSet<MenuItem> MenuItems { get; set; }
    public virtual DbSet<Merchant> Merchants { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<OrderTracking> OrderTrackings { get; set; }
    public virtual DbSet<Promotion> Promotions { get; set; }
    public virtual DbSet<Restaurant> Restaurants { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<SearchHistory> SearchHistories { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<CustomerVoucher> CustomerVouchers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerVoucher>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CustomerId, e.VoucherId }).IsUnique();
        });
        
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasIndex(e => e.CustomerId, "IX_Addresses_CustomerId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(d => d.Customer).WithMany(p => p.Addresses).HasForeignKey(d => d.CustomerId);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasIndex(e => e.CustomerId, "IX_Carts_CustomerId").IsUnique();
            entity.HasIndex(e => e.RestaurantId, "IX_Carts_RestaurantId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(d => d.Customer).WithOne(p => p.Cart).HasForeignKey<Cart>(d => d.CustomerId);
            entity.HasOne(d => d.Restaurant).WithMany(p => p.Carts).HasForeignKey(d => d.RestaurantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(e => e.CartId, "IX_CartItems_CartId");
            entity.HasIndex(e => e.MenuItemId, "IX_CartItems_MenuItemId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems).HasForeignKey(d => d.CartId);
            entity.HasOne(d => d.MenuItem).WithMany(p => p.CartItems).HasForeignKey(d => d.MenuItemId);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_ChatMessages_OrderId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(d => d.Order).WithMany(p => p.ChatMessages).HasForeignKey(d => d.OrderId);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Customers_UserId").IsUnique();
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(d => d.User).WithOne(p => p.Customer).HasForeignKey<Customer>(d => d.UserId);
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Drivers_UserId").IsUnique();
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.Property(e => e.WalletBalance).HasPrecision(18, 2);
            entity.HasOne(d => d.User).WithOne(p => p.Driver).HasForeignKey<Driver>(d => d.UserId);
        });

        modelBuilder.Entity<DriverEarning>(entity =>
        {
            entity.HasIndex(e => e.DriverId, "IX_DriverEarnings_DriverId");
            entity.HasIndex(e => e.OrderId, "IX_DriverEarnings_OrderId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(d => d.Driver).WithMany(p => p.DriverEarnings).HasForeignKey(d => d.DriverId);
            entity.HasOne(d => d.Order).WithMany(p => p.DriverEarnings).HasForeignKey(d => d.OrderId);
        });

        modelBuilder.Entity<FoodCategory>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<MenuCategory>(entity =>
        {
            entity.HasIndex(e => e.RestaurantId, "IX_MenuCategories_RestaurantId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(d => d.Restaurant).WithMany(p => p.MenuCategories).HasForeignKey(d => d.RestaurantId);
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasIndex(e => e.MenuCategoryId, "IX_MenuItems_MenuCategoryId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasOne(d => d.MenuCategory).WithMany(p => p.MenuItems).HasForeignKey(d => d.MenuCategoryId);
        });

        modelBuilder.Entity<Merchant>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Merchants_UserId").IsUnique();
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.HasOne(d => d.User).WithOne(p => p.Merchant).HasForeignKey<Merchant>(d => d.UserId);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Notifications_UserId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(d => d.User).WithMany(p => p.Notifications).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.CustomerId, "IX_Orders_CustomerId");
            entity.HasIndex(e => e.DriverId, "IX_Orders_DriverId");
            entity.HasIndex(e => e.OrderNumber, "IX_Orders_OrderNumber").IsUnique();
            entity.HasIndex(e => e.RestaurantId, "IX_Orders_RestaurantId");
            entity.HasIndex(e => e.VoucherId, "IX_Orders_VoucherId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DeliveryFee).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.HasOne(d => d.Customer).WithMany(p => p.Orders).HasForeignKey(d => d.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Driver).WithMany(p => p.Orders).HasForeignKey(d => d.DriverId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(d => d.Restaurant).WithMany(p => p.Orders).HasForeignKey(d => d.RestaurantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Voucher).WithMany(p => p.Orders).HasForeignKey(d => d.VoucherId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(e => e.MenuItemId, "IX_OrderItems_MenuItemId");
            entity.HasIndex(e => e.OrderId, "IX_OrderItems_OrderId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(d => d.MenuItem).WithMany(p => p.OrderItems).HasForeignKey(d => d.MenuItemId);
            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasForeignKey(d => d.OrderId);
        });

        modelBuilder.Entity<OrderTracking>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_OrderTrackings_OrderId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(d => d.Order).WithMany(p => p.OrderTrackings).HasForeignKey(d => d.OrderId);
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasIndex(e => e.RestaurantId, "IX_Promotions_RestaurantId");
            entity.HasIndex(e => e.VoucherId, "IX_Promotions_VoucherId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(d => d.Restaurant).WithMany(p => p.Promotions).HasForeignKey(d => d.RestaurantId);
            entity.HasOne(d => d.Voucher).WithMany(p => p.Promotions).HasForeignKey(d => d.VoucherId);
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasIndex(e => e.MerchantId, "IX_Restaurants_MerchantId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.Property(e => e.IsTrending).HasDefaultValue(false);
            entity.Property(e => e.TotalOrders).HasDefaultValue(0);
            entity.HasOne(d => d.Merchant).WithMany(p => p.Restaurants).HasForeignKey(d => d.MerchantId);
            
            // New Mappings
            entity.HasOne(d => d.Category).WithMany().HasForeignKey(d => d.CategoryId);
            entity.Property(e => e.Tags).HasColumnType("text[]"); // or similar for PostgreSQL
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasIndex(e => e.CustomerId, "IX_Reviews_CustomerId");
            entity.HasIndex(e => e.OrderId, "IX_Reviews_OrderId").IsUnique();
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(d => d.Customer).WithMany(p => p.Reviews).HasForeignKey(d => d.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Order).WithOne(p => p.Review).HasForeignKey<Review>(d => d.OrderId);
        });

        modelBuilder.Entity<SearchHistory>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_SearchHistories_UserId");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(d => d.User).WithMany(p => p.SearchHistories).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique().HasFilter("(\"Email\" IS NOT NULL)");
            entity.HasIndex(e => e.PhoneNumber, "IX_Users_PhoneNumber").IsUnique();
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasIndex(e => e.Code, "IX_Vouchers_Code").IsUnique();
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
            entity.Property(e => e.MaxDiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.MinOrderAmount).HasPrecision(18, 2);
        });
    }
}
