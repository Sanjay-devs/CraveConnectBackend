

using CraveConnect.Entity;
using Microsoft.EntityFrameworkCore;
using Test.Entity;
using Test.Model;
using Test.Utilities;

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
            builder.Entity<UserTypeMasterEntity>().ToTable("UserTypeMaster");


            builder.Entity<DBCountResponse>().HasNoKey();
            builder.Entity<RestaurantModel>().HasNoKey();
            builder.Entity<MenuItemModel>().HasNoKey();
            builder.Entity<FoodItemModel>().HasNoKey();
            builder.Entity<UserModel>().HasNoKey();
            //builder.Entity<MostVisitedRestaurant>().HasNoKey();
            //builder.Entity<MostOrderedFood>().HasNoKey();
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RestaurantEntity> Restaurants { get; set; }
        public DbSet<MenuItemEntity> MenuItem { get; set; }
        public DbSet<FoodItemEntity> FoodItems { get; set; }
        public DbSet<CartEntity> Cart { get; set; }
        public DbSet<OrdersEntity> Order { get; set; }
        public DbSet<UserTypeMasterEntity> UserType { get; set; }
        public DbSet<MostOrderedFood> MostOrderedFoods { get; set; }
        public DbSet<MostVisitedRestaurant> MostVisitedRestaurants { get; set; }

    }
}
