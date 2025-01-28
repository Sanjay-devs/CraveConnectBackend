using System.ComponentModel.DataAnnotations;

namespace Test.Entity
{
    public class FoodItemEntity
    {
        [Key]
        public int FoodItemId { get; set; }
        public int RestaurantId { get; set; }
        public int MenuId { get; set; }
        public string? FoodItem { get; set; }
        public decimal? Price { get; set; }
        public string? FoodImage { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }
}
