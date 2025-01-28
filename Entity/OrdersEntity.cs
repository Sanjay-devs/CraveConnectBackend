using System.ComponentModel.DataAnnotations;

namespace Test.Entity
{
    public class OrdersEntity
    {
        [Key]
        public int OrderId { get; set; }
        public int? UserId { get; set; }
        public int MenuId { get; set; }
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public string? PaymentStatus { get; set; }
        public int FoodItemId { get; set; }
        public string? FoodItem { get; set; }
        public string? FoodImage { get; set; }
        public int CartId { get; set; }
        public int TotalQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }
}
