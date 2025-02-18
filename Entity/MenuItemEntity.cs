using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Test.Model;

namespace Test.Entity
{
    public class MenuItemEntity
    {
        [Key]
        public int MenuId { get; set; }
        public int RestaurantId { get; set; }
        public string? ItemName { get; set; }
        public decimal Price { get; set; }
        public string? MenuImage { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }

    public class MenuItemList : MenuItemEntity
    {
        public int MenuId { get; set; }
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public string? ItemName { get; set; }
        public decimal Price { get; set; }
        public string? MenuImage { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }

    //public class MenuItemspModel : Pagenation
    //{
    //    public List<MenuItemList> result { get; set; }
    //    public int count { get; set; }

    //}
}
