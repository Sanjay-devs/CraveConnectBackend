

using Microsoft.EntityFrameworkCore;
using Test.Entity;

namespace Test.Context
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserEntity>().ToTable("UserMaster");
            builder.Entity<RestaurantEntity>().ToTable("Restaurants");
            builder.Entity<MenuItemEntity>().ToTable("MenuItems");
            builder.Entity<FoodItemEntity>().ToTable("FoodItems");
            builder.Entity<CartEntity>().ToTable("Carts");
            builder.Entity<OrdersEntity>().ToTable("Orders");
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RestaurantEntity> Restaurants { get; set; }
        public DbSet<MenuItemEntity> MenuItem { get; set; }
        public DbSet<FoodItemEntity> FoodItems { get; set; }
        public DbSet<CartEntity> Cart { get; set; }
        public DbSet<OrdersEntity> Order { get; set; }
        
    }
}
