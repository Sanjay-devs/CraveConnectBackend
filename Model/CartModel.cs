namespace Test.Model
{
    public class CartModel
    {
        public int CartId { get; set; }
        public int FoodItemID { get; set; }
        public string? FoodItem { get; set; }
        public string? FoodImage { get; set; }
        public int MenuId { get; set; }
        public int RestaurantId { get; set; }
        public int UserId { get; set; }
        public int TotalCount { get; set; }
        public decimal? Price { get; set; }
        public decimal? TotalPrice { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
