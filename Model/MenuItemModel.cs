namespace Test.Model
{
    public class MenuItemModel
    {
        public int MenuId { get; set; }
        public int RestaurantId { get; set; }
        public string? ItemName { get; set; }
        public decimal Price { get; set; }
        public string? MenuImage { get; set; }
        //public bool IsDeleted { get; set; }
        //public bool IsActive { get; set; }
    }
}
