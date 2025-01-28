namespace Test.Model
{
    public class OrdersModel
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int MenuId { get; set; }
        public int RestaurantId { get; set; }
        public int FoodItemId { get; set; }
        public string? FoodItem { get; set; }
        public string? FoodImage { get; set; }
        public int CartId { get; set; }
        public int TotalQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
