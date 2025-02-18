    using System.ComponentModel.DataAnnotations;

namespace Test.Model
{
    public class FoodItemModel
    {
        public int FoodItemId { get; set; }
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public int MenuId { get; set; }
        public string? ItemName { get; set; }
        public string? FoodItem { get; set; }
        public decimal? Price { get; set; }
        public string? FoodImage { get; set; }
    }

    public class FoodItemspModel : Pagenation
    {
        public List<FoodItemModel> result { get; set; }
        public int count { get; set; }

    }

    public class MostOrderedFood : FoodItemModel
    {
        public int FoodItemId { get; set; }
        public string? FoodItem { get; set; }
        public int OrderCount { get; set; }
    }

    
}
