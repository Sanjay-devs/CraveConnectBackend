using System.ComponentModel.DataAnnotations;
using Test.Entity;

namespace Test.Model
{
    public class UserModel
    {
        public int UserId { get; set; }
        public int UserTypeId { get; set; }
        public string? UserTypeName { get; set; }
        public string? EmailId { get; set; }
        public string? Password { get; set; }
        public string? VerifyPassword { get; set; }
        public string? UserName { get; set; }
        public string? Address { get; set; }
        public string? MobileNumber { get; set; }
        public string? Image { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        //public DateTime CreatedOn { get; set; }
        //public DateTime UpdatedOn { get; set; }
    }

    public class Pagenation
    {
        public int pageNumber { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public int totalCount { get; set; } = 0;
        public string q { get; set; } = "";

    }
    public class DBCountResponse
    {
        public int cnt { get; set; }
        public int totalCount { get; set; }
    }

    public class UserspModel : Pagenation
    {
        public List<UserModel> result { get; set; }
        public int count { get; set; }

    }
}
