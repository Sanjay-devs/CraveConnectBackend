using System.ComponentModel.DataAnnotations;
using Test.Model;

namespace Test.Entity
{
    public class RestaurantEntity
    {
        [Key]
        public int RestaurantId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? Rest_Image { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }

    public class RestaurantspModel : Pagenation
    {
        public List<RestaurantEntity> result { get; set; }
        public int count { get; set; }

    }
}
